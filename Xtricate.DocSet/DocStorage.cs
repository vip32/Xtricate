using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dapper;
using Xtricate.DocSet.Logging;

namespace Xtricate.DocSet
{
    public class DocStorage<TDoc> : IDocStorage<TDoc>
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(DocStorage<TDoc>));
        protected readonly IDbConnectionFactory ConnectionFactory;
        protected readonly IHasher Hasher;
        protected readonly IEnumerable<IIndexMap<TDoc>> IndexMaps;
        protected readonly IStorageOptions Options;
        protected readonly ISerializer Serializer;
        protected readonly ISqlBuilder SqlBuilder;
        protected string TableName;

        public DocStorage(IDbConnectionFactory connectionFactory, IStorageOptions options, ISqlBuilder sqlBuilder,
            ISerializer serializer = null, IHasher hasher = null, IEnumerable<IIndexMap<TDoc>> indexMap = null)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (sqlBuilder == null) throw new ArgumentNullException(nameof(sqlBuilder));

            ConnectionFactory = connectionFactory;
            Options = options;
            SqlBuilder = sqlBuilder;
            Serializer = serializer ?? new JsonNetSerializer();
            Hasher = hasher;// ?? new Md5Hasher();
            TableName = options.GetTableName<TDoc>();
            IndexMaps = indexMap.NullToEmpty().ToList().Where(im => im != null).OrderBy(i => i.Name);

            if (Options.EnableLogging)
            {
                Log.Debug($"table: {TableName}");
                IndexMaps.ForEach(
                    im => Log.Debug($"index map: {typeof (TDoc).Name} > {im.Name} [{im.Description}]"));
                Log.Debug($"connection: {Options.ConnectionString}");
            }
            Initialize();
        }

        public virtual void Reset(bool indexOnly = false)
        {
            if (!indexOnly)
                DeleteTable(TableName);
            DeleteIndex(TableName);
            Initialize();
        }

        public virtual void Execute(Action action)
        {
            if (Options.UseTransactions)
                using (var trans = CreateConnection().BeginTransaction())
                {
                    action();
                    trans.Commit();
                }
            else
                action();
        }

        public virtual bool Exists(object key, IEnumerable<string> tags = null)
        {
            //Log.Debug($"document exists: key={key},tags={tags?.ToString("||")}");
            var sql = $@"
    SELECT [id] FROM {TableName} WHERE [key]='{key}'";
            tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));

            using (var conn = CreateConnection())
            {
                conn.Open();
                return conn.Query<int>(sql, new {key}).Any();
            }
        }

        public virtual StorageAction Upsert(object key, TDoc document, IEnumerable<string> tags = null,
            bool forceInsert = false, DateTime? timestamp = null)
        {
            return UpsertInternal(key, document: document, tags: tags, forceInsert: forceInsert, timestamp: timestamp);
        }

        public virtual StorageAction Upsert(object key, Stream data, IEnumerable<string> tags = null,
            bool forceInsert = false, DateTime? timestamp = null)
        {
            return UpsertInternal(key, data: data, tags: tags, forceInsert: forceInsert, timestamp: timestamp);
        }

        private StorageAction UpsertInternal(object key, TDoc document = default(TDoc), Stream data = null, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            // http://www.databasejournal.com/features/mssql/using-the-merge-statement-to-perform-an-upsert.html
            // http://stackoverflow.com/questions/2479488/syntax-for-single-row-merge-upsert-in-sql-server
            using (var conn = CreateConnection())
            {
                string sql;
                StorageAction result;
                if (!forceInsert && Exists(key, tags))
                {
                    // UPDATE ===
                    if (Options.EnableLogging)
                        Log.Debug($"{TableName} update: key={key},tags={tags?.ToString("||")}");
                    var updateColumns = "[value]=@value";
                    if (document != null && data != null) updateColumns += ",[data]=@data";
                    if (document == null && data != null) updateColumns = "[data]=@data";
                    sql =
                        $@"
    UPDATE {TableName}
    SET [hash]=@hash,[timestamp]=@timestamp,{updateColumns}
        {
                            IndexMaps.NullToEmpty()
                                .Select(
                                    i =>
                                        ",[" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix + "]=@" +
                                        i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix)
                                .ToString("")}
    WHERE [key]=@key";
                    tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                    result = StorageAction.Updated;
                }
                else
                {
                    // INSERT ===
                    if (Options.EnableLogging)
                        Log.Debug($"{TableName} insert: key={key},tags={tags?.ToString("||")}");
                    var insertColumns = "[value]";
                    if (document != null && data != null) insertColumns += ",[data]";
                    if (document == null && data != null) insertColumns = "[data]";
                    var insertValues = "@value";
                    if (document != null && data != null) insertValues += ",@data";
                    if (document == null && data != null) insertValues = "@data";
                    sql =
                        $@"
    INSERT INTO {TableName}
        ([key],[tags],[hash],[timestamp],{insertColumns}{
                            IndexMaps.NullToEmpty()
                                .Select(i => ",[" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix + "]")
                                .ToString("")})
        VALUES(@key,@tags,@hash,@timestamp,{insertValues}{
                            IndexMaps.NullToEmpty()
                                .Select(i => ",@" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix)
                                .ToString("")})";
                    result = StorageAction.Inserted;
                }

                // PARAMS
                var parameters = new DynamicParameters();
                parameters.Add("key", key.ToString());
                parameters.Add("tags", $"||{tags.ToString("||")}||");
                parameters.Add("hash", Hasher?.Compute(document));
                parameters.Add("timestamp", timestamp ?? DateTime.UtcNow);
                parameters.Add("value", Serializer.ToJson(document));
                parameters.Add("data", data.ToBytes().Compress(), DbType.Binary);
                AddIndexParameters(document, parameters);

                conn.Open();
                conn.Execute(sql, parameters);
                return result;
            }
        }

        public virtual long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} count: tags={tags?.ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"SELECT COUNT(*) FROM {TableName} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                return conn.Query<int>(sql).SingleOrDefault();
            }
        }

        public virtual IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (Options.EnableLogging)
                Log.Debug(
                $"{TableName} load: key={key}, tags={tags?.ToString("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"SELECT [value] FROM {TableName} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql += SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime);
                sql += SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn);
                sql += SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize);
                conn.Open();
                var results = conn.Query<string>(sql, new {key}, buffered: Options.BufferedLoad);
                if (results == null) yield break;
                foreach (var result in results)
                    yield return Serializer.FromJson<TDoc>(result);
            }
        }

        public virtual IEnumerable<Stream> LoadData(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (Options.EnableLogging)
                Log.Debug(
                $"{TableName} load: key={key}, tags={tags?.ToString("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"SELECT [data] FROM {TableName} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql += SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime);
                sql += SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn);
                sql += SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize);
                conn.Open();
                var results = conn.Query<byte[]>(sql, new {key}, buffered: Options.BufferedLoad);
                if (results == null) yield break;
                foreach (var data in results.Where(data => data != null))
                    yield return new MemoryStream(data.Decompress());
            }
        }

        public virtual IEnumerable<TDoc> Load(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (Options.EnableLogging)
                Log.Debug(
                $"{TableName} load: tags={tags?.ToString("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"SELECT [value] FROM {TableName} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql += SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime);
                sql += SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn);
                sql += SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize);
                conn.Open();
                var documents = conn.Query<string>(sql, buffered: Options.BufferedLoad);
                if (documents == null) yield break;
                foreach (var document in documents)
                    yield return Serializer.FromJson<TDoc>(document);
            }
        }

        public virtual StorageAction Delete(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} delete: key={key},tags={tags?.ToString("||")}");
            using (var conn = CreateConnection())
            {
                var sql = $@"DELETE FROM {TableName} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                var num = conn.Execute(sql, new {key});
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
            }
        }

        public virtual StorageAction Delete(IEnumerable<string> tags,
            IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} delete: tags={tags?.ToString("||")}");
            if (tags.IsNullOrEmpty()) return StorageAction.None;
            using (var conn = CreateConnection())
            {
                var sql = $@"DELETE FROM {TableName} WHERE ";
                tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                var num = conn.Execute(sql);
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
            }
        }

        private IDbConnection CreateConnection() => ConnectionFactory.CreateConnection(Options.ConnectionString);

        public void Initialize()
        {
            TableName = Options.GetTableName<TDoc>();

            EnsureSchema(Options);
            EnsureTable(TableName);
            EnsureIndex(TableName);
        }

        private void AddIndexParameters(TDoc document, DynamicParameters parameters)
        {
            if (document == null)
            {
                AddNullIndexParameters(parameters);
                return;
            }
            if (IndexMaps.IsNullOrEmpty()) return;
            if (parameters == null) parameters = new DynamicParameters();
            var indexColumnValues = IndexMaps.ToDictionary(i => i.Name,
                i => i.Value != null
                    ? $"||{i.Value(document)}||"
                    : $"||{i.Values(document).ToString("||")}||");

            foreach (var item in IndexMaps)
            {
                parameters.Add(item.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix,
                    indexColumnValues.FirstOrDefault(
                        i => i.Key.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))
                        .ValueOrDefault(i => i.Value));
            }
        }

        private void AddNullIndexParameters(DynamicParameters parameters)
        {
            if (IndexMaps.IsNullOrEmpty()) return;
            if (parameters == null) parameters = new DynamicParameters();

            foreach (var item in IndexMaps)
            {
                parameters.Add(item.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix, null);
            }
        }

        protected virtual bool TableExists(string tableName)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} exists [{conn.Database}]");
                return
                    conn.Query<string>(SqlBuilder.TableNamesSelect())
                        .Any(t => t.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        protected virtual void EnsureSchema(IStorageOptions options)
        {
            if (string.IsNullOrEmpty(options.SchemaName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{options.SchemaName} ensure schema [{conn.Database}]");
                if (conn.Query<string>(@"
    SELECT QUOTENAME(TABLE_SCHEMA) AS Name
    FROM INFORMATION_SCHEMA.TABLES")
                    .Any(t =>
                        t.Equals($"[{options.SchemaName}]",
                            StringComparison.InvariantCultureIgnoreCase)))
                    return;
                try
                {
                    conn.Execute($"CREATE SCHEMA [{options.SchemaName}] AUTHORIZATION dbo");
                }
                catch (SqlException e)
                {
                    // swallog
                    if (Options.EnableLogging)
                        Log.Warn($"create schema: {e.Message}: ");
                }
            }
        }

        protected virtual void EnsureTable(string tableName)
        {
            if (TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} ensure table [{conn.Database}]");
                // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                var sql = string.Format(@"
    CREATE TABLE {0}(
    [uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
    [id] INTEGER NOT NULL IDENTITY(1,1),
    [key] NVARCHAR(512) NOT NULL,
    [tags] NVARCHAR(1024) NOT NULL,
    [hash] NVARCHAR(128),
    [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [value] TEXT,
    [data] VARBINARY(MAX));

    CREATE UNIQUE CLUSTERED INDEX [IX_id_{1}] ON {0} (id)
    CREATE INDEX [IX_key_{1}] ON {0} ([key] ASC);
    CREATE INDEX [IX_tags_{1}] ON {0} ([tags] ASC);
    CREATE INDEX [IX_hash_{1}] ON {0} ([hash] ASC);",
                    tableName, new Random().Next(1000, 9999));
                conn.Execute(sql);
            }
        }

        protected virtual void EnsureIndex(string tableName)
        {
            if (IndexMaps == null || !IndexMaps.Any()) return;
            if (!TableExists(tableName)) EnsureTable(tableName);
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug(
                    $"{tableName} ensure index [{conn.Database}], index={IndexMaps.NullToEmpty().Select(i => i.Name).ToString(", ")}");
                var sql = IndexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    IF NOT EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}{2}' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        ALTER TABLE {0} ADD [{1}{2}] NVARCHAR(2048)
        CREATE INDEX [IX_{1}{2}] ON {0} ([{1}{2}] ASC)
    END ", tableName, i.Name.ToLower(), SqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => conn.Execute(s));
            }
            // sqlite check column exists: http://stackoverflow.com/questions/18920136/check-if-a-column-exists-in-sqlite
            // sqlite alter table https://www.sqlite.org/lang_altertable.html
        }

        private void DeleteTable(string tableName)
        {
            if (!TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} drop table [{conn.Database}]");
                var sql = string.Format(@"DROP TABLE {0}", tableName);
                conn.Execute(sql);
            }
        }

        private void DeleteIndex(string tableName)
        {
            if (!TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} drop table [{conn.Database}]");
                var sql = IndexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    IF EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}{2}' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        DROP INDEX {0}.[IX_{1}{2}]
        ALTER TABLE {0} DROP COLUMN [{1}{2}]
    END ", tableName, i.Name.ToLower(), SqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => conn.Execute(s));
            }
        }
    }
}