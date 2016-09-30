using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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

            var builder = new SqlConnectionStringBuilder(options.ConnectionString);
            options.DataSource = builder.DataSource;
            if (!builder.InitialCatalog.IsNullOrEmpty()) // overrule database name if provided in connectionstring
                Options.DatabaseName = builder.InitialCatalog;

            SqlBuilder = sqlBuilder;
            Serializer = serializer ?? new JsonNetSerializer();
            Hasher = hasher;// ?? new Md5Hasher();
            TableName = options.GetTableName<TDoc>();
            IndexMaps = indexMap.NullToEmpty().ToList().Where(im => im != null).OrderBy(i => i.Name);

            if (Options.EnableLogging)
            {
                Log.Debug($"datasource: {Options.DataSource}");
                Log.Debug($"database: {Options.DatabaseName}");
                Log.Debug($"schema: {Options.SchemaName}");
                Log.Debug($"table: {TableName}");
                IndexMaps.ForEach(
                    im => Log.Debug($"index map: {typeof(TDoc).Name} > {im.Name} [{im.Description}]"));
                //Log.Debug($"connection: {Options.ConnectionString}");
            }

            Initialize();
        }

        public virtual void Reset(bool indexOnly = false)
        {
            if (!indexOnly)
                DeleteTable(Options.DatabaseName, TableName);
            DeleteIndex(Options.DatabaseName, TableName);
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
            //Log.Debug($"document exists: key={key},tags={tags?.Join("||")}");

            var sql = new StringBuilder($@"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} SELECT [id] FROM {TableName} WHERE [key]='{key}'");
            foreach (var t in tags.NullToEmpty())
                sql.Append(SqlBuilder.BuildTagSelect(t));
            //var sql = $@"SELECT [id] FROM {TableName} WHERE [key]='{key}'";
            //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));

            using (var conn = CreateConnection())
            {
                conn.Open();
                return conn.Query<int>(sql.ToString(), new { key }).Any();
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

        public virtual StorageAction Upsert(object key, TDoc document, Stream data, IEnumerable<string> tags = null,
            bool forceInsert = false, DateTime? timestamp = null)
        {
            return UpsertInternal(key, document: document, data: data, tags: tags, forceInsert: forceInsert, timestamp: timestamp);
        }

        private StorageAction UpsertInternal(object key, TDoc document = default(TDoc), Stream data = null, IEnumerable<string> tags = null, bool forceInsert = false, DateTime? timestamp = null)
        {
            // http://www.databasejournal.com/features/mssql/using-the-merge-statement-to-perform-an-upsert.html
            // http://stackoverflow.com/questions/2479488/syntax-for-single-row-merge-upsert-in-sql-server
            using (var conn = CreateConnection())
            {
                StorageAction result;
                var sql = new StringBuilder();
                if (!forceInsert && Exists(key, tags))
                {
                    result = Update(key, document, data, tags, sql);
                }
                else
                {
                    result = Insert(key, document, data, tags, sql);
                }

                // PARAMS
                var parameters = new DynamicParameters();
                parameters.Add("key", key.ToString());
                parameters.Add("tags", $"||{tags.Join("||")}||");
                parameters.Add("hash", Hasher?.Compute(document));
                parameters.Add("timestamp", timestamp ?? DateTime.UtcNow);
                parameters.Add("value", Serializer.ToJson(document));
                parameters.Add("data", data.ToBytes().Compress(), DbType.Binary);
                AddIndexParameters(document, parameters);

                conn.Open();
                conn.Execute(sql.ToString(), parameters);
                return result;
            }
        }

        private StorageAction Update(object key, TDoc document, Stream data, IEnumerable<string> tags, StringBuilder sql)
        {
            // UPDATE ===
            if (Options.EnableLogging)
                Log.Debug($"{TableName} update: key={key},tags={tags?.Join("||")}");
            var updateColumns = "[value]=@value";
            if (document != null && data != null) updateColumns = $"{updateColumns},[data]=@data";
            if (document == null && data != null) updateColumns = "[data]=@data";
            sql.Append(
                $@"
    {SqlBuilder.BuildUseDatabase(Options.DatabaseName)}
    UPDATE {TableName
                    }
    SET [hash]=@hash,[timestamp]=@timestamp,{updateColumns
                    }
        {
                    IndexMaps.NullToEmpty()
                        .Select(
                            i =>
                                ",[" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix + "]=@" +
                                i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix)
                        .Join("")}
    WHERE [key]=@key");
            foreach (var t in tags.NullToEmpty())
                sql.Append(SqlBuilder.BuildTagSelect(t));
            return StorageAction.Updated;
        }

        private StorageAction Insert(object key, TDoc document, Stream data, IEnumerable<string> tags, StringBuilder sql)
        {
            // INSERT ===
            if (Options.EnableLogging)
                Log.Debug($"{TableName} insert: key={key},tags={tags?.Join("||")}");
            var insertColumns = "[value]";
            if (document != null && data != null) insertColumns = $"{insertColumns},[data]" /*+= ",[data]"*/;
            if (document == null && data != null) insertColumns = "[data]";
            var insertValues = "@value";
            if (document != null && data != null) insertValues = $"{insertValues},@data" /*",@data"*/;
            if (document == null && data != null) insertValues = "@data";
            sql.Append(
                $@"
    {SqlBuilder.BuildUseDatabase(Options.DatabaseName)}
    INSERT INTO {TableName
                    }
        ([key],[tags],[hash],[timestamp],{insertColumns}{
                    IndexMaps.NullToEmpty()
                        .Select(i => ",[" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix + "]")
                        .Join("")})
        VALUES(@key,@tags,@hash,@timestamp,{insertValues}{
                    IndexMaps.NullToEmpty()
                        .Select(i => ",@" + i.Name.ToLower() + SqlBuilder.IndexColumnNameSuffix)
                        .Join("")})");
            return StorageAction.Inserted;
        }

        public virtual long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} count: tags={tags?.Join("||")}");

            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($@"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} SELECT COUNT(*) FROM {TableName} WHERE [id]>0");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //var sql = $@"SELECT COUNT(*) FROM {TableName} WHERE [id]>0";
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                return conn.Query<int>(sql.ToString()).SingleOrDefault();
            }
        }

        public virtual IEnumerable<object> LoadKeys(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} count: tags={tags?.Join("||")}");

            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($@"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} SELECT [key] FROM {TableName} WHERE [id]>0");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //var sql = $@"SELECT [key] FROM {TableName} WHERE [id]>0";
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                return conn.Query<object>(sql.ToString());
            }
        }

        public virtual IEnumerable<TDoc> LoadValues(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (Options.EnableLogging)
                Log.Debug(
                $"{TableName} load: key={key}, tags={tags?.Join("||")}, criterias={criterias?.Select(c => $"{c.Name}:{c.Value}").Join("||")}");

            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.BuildValueSelectByKey(TableName)}");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql.Append(SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                sql.Append(SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn));
                sql.Append(SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize));
                //var sql = SqlBuilder.BuildValueSelectByKey(TableName);
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //sql += SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime);
                //sql += SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn);
                //sql += SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize);
                conn.Open();
                var results = conn.Query<string>(sql.ToString(), new { key }, buffered: Options.BufferedLoad);
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
                $"{TableName} load: key={key}, tags={tags?.Join("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).Join("||")}");

            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.BuildDataSelectByKey(TableName)}");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql.Append(SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                sql.Append(SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn));
                sql.Append(SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize));
                //var sql = SqlBuilder.BuildDataSelectByKey(TableName);
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //sql += SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime);
                //sql += SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn);
                //sql += SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize);
                conn.Open();
                var results = conn.Query<byte[]>(sql.ToString(), new { key }, buffered: Options.BufferedLoad);
                if (results == null) yield break;
                foreach (var data in results.Where(data => data != null))
                    yield return new MemoryStream(data.Decompress());
            }
        }

        public virtual IEnumerable<TDoc> LoadValues(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} load: tags={tags?.Join("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).Join("||")}");

            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.BuildValueSelectByTags(TableName)}");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                sql.Append(SqlBuilder.BuildFromTillDateTimeSelect(fromDateTime, tillDateTime));
                sql.Append(SqlBuilder.BuildSortingSelect(Options.DefaultSortColumn));
                sql.Append(SqlBuilder.BuildPagingSelect(skip, take, Options.DefaultTakeSize, Options.MaxTakeSize));
                conn.Open();
                var documents = conn.Query<string>(sql.ToString(), buffered: Options.BufferedLoad);
                if (documents == null) yield break;
                foreach (var document in documents)
                    yield return Serializer.FromJson<TDoc>(document);
            }
        }

        public virtual StorageAction Delete(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} delete: key={key},tags={tags?.Join("||")}");
            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.BuildDeleteByKey(TableName)}");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //var sql = SqlBuilder.BuildDeleteByKey(TableName);
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                var num = conn.Execute(sql.ToString(), new { key });
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
            }
        }

        public virtual StorageAction Delete(IEnumerable<string> tags,
            IEnumerable<Criteria> criterias = null)
        {
            if (Options.EnableLogging)
                Log.Debug($"{TableName} delete: tags={tags?.Join("||")}");
            if (tags.IsNullOrEmpty()) return StorageAction.None;
            using (var conn = CreateConnection())
            {
                var sql = new StringBuilder($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.BuildDeleteByTags(TableName)}");
                foreach (var t in tags.NullToEmpty())
                    sql.Append(SqlBuilder.BuildTagSelect(t));
                foreach (var c in criterias.NullToEmpty())
                    sql.Append(SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                //var sql = SqlBuilder.BuildDeleteByTags(TableName);
                //tags.NullToEmpty().ForEach(t => sql += SqlBuilder.BuildTagSelect(t));
                //criterias.NullToEmpty().ForEach(c => sql += SqlBuilder.BuildCriteriaSelect(IndexMaps, c));
                conn.Open();
                var num = conn.Execute(sql.ToString());
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
            }
        }

        private IDbConnection CreateConnection() => ConnectionFactory.CreateConnection(Options.ConnectionString);

        public void Initialize()
        {
            TableName = Options.GetTableName<TDoc>();

            EnsureDatabase(Options);
            EnsureSchema(Options);
            EnsureTable(Options.DatabaseName, TableName);
            EnsureIndex(Options.DatabaseName, TableName);
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
                    : $"||{i.Values(document).Join("||")}||");

            foreach (var item in IndexMaps)
            {
                parameters.Add($"{item.Name.ToLower()}{SqlBuilder.IndexColumnNameSuffix}",
                    indexColumnValues.FirstOrDefault(
                        i => i.Key.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                        .ValueOrDefault(i => i.Value));
            }
        }

        private void AddNullIndexParameters(DynamicParameters parameters)
        {
            if (IndexMaps.IsNullOrEmpty()) return;
            if (parameters == null) parameters = new DynamicParameters();

            foreach (var item in IndexMaps)
            {
                parameters.Add($"{item.Name.ToLower()}{SqlBuilder.IndexColumnNameSuffix}", null);
            }
        }

        protected virtual bool TableExists(string databaseName, string tableName)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} exists [{conn.Database}]");
                return
                    conn.Query<string>($"{SqlBuilder.BuildUseDatabase(Options.DatabaseName)} {SqlBuilder.TableNamesSelect()}")
                        .Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            }
        }

        protected virtual void EnsureDatabase(IStorageOptions options)
        {
            if (string.IsNullOrEmpty(options.DatabaseName)) return;
            using (var conn = CreateConnection())
            {
                if (Options.EnableLogging)
                    Log.Debug($"{options.DatabaseName} ensure database [{conn.Database}]");

                EnsureOpenConnection(conn, options);

                if (conn.Query<string>($@"
    SELECT *
    FROM sys.databases
    WHERE name='{options.DatabaseName}'")
                    .Any())
                    return;
                try
                {
                    conn.Execute($"CREATE DATABASE [{options.DatabaseName}]");
                }
                catch (SqlException e)
                {
                    // swallog
                    if (Options.EnableLogging)
                        Log.Warn($"create database: {e.Message}");
                }
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
                if (conn.Query<string>($@"
    {SqlBuilder.BuildUseDatabase(options.DatabaseName)}
    SELECT QUOTENAME(SCHEMA_NAME) AS Name
    FROM INFORMATION_SCHEMA.SCHEMATA")
                    .Any(t =>
                        t.Equals($"[{options.SchemaName}]",
                            StringComparison.OrdinalIgnoreCase)))
                    return;
                try
                {
                    conn.Execute($"CREATE SCHEMA [{options.SchemaName}] AUTHORIZATION dbo");
                }
                catch (SqlException e)
                {
                    // swallog
                    if (Options.EnableLogging)
                        Log.Warn($"create schema: {e.Message}");
                }
            }
        }

        protected virtual void EnsureTable(string databaseName, string tableName)
        {
            if (TableExists(databaseName, tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} ensure table [{conn.Database}]");
                // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                var sql = string.Format(@"
    {0}
    CREATE TABLE {1}(
    [uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
    [id] INTEGER NOT NULL IDENTITY(1,1),
    [key] NVARCHAR(512) NOT NULL,
    [tags] NVARCHAR(1024) NOT NULL,
    [hash] NVARCHAR(128),
    [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [value] NTEXT,
    [data] VARBINARY(MAX));

    CREATE UNIQUE CLUSTERED INDEX [IX_id_{2}] ON {1} (id)
    CREATE INDEX [IX_key_{2}] ON {1} ([key] ASC);
    CREATE INDEX [IX_tags_{2}] ON {1} ([tags] ASC);
    CREATE INDEX [IX_hash_{2}] ON {1} ([hash] ASC);",
                    SqlBuilder.BuildUseDatabase(databaseName),
                    tableName, new Random().Next(1000, 9999).ToString());
                conn.Execute(sql);
            }
        }

        protected virtual void EnsureIndex(string databaseName, string tableName)
        {
            if (IndexMaps == null || !IndexMaps.Any()) return;
            if (!TableExists(databaseName, tableName)) EnsureTable(databaseName, tableName);
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug(
                    $"{tableName} ensure index [{conn.Database}], index={IndexMaps.NullToEmpty().Select(i => i.Name).Join(", ")}");
                var sql = IndexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    {0}
    IF NOT EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{2}{3}' AND Object_ID = Object_ID(N'{1}'))
    BEGIN
        ALTER TABLE {1} ADD [{2}{3}] NVARCHAR(2048)
        CREATE INDEX [IX_{2}{3}] ON {1} ([{2}{3}] ASC)
    END ", SqlBuilder.BuildUseDatabase(databaseName), tableName, i.Name.ToLower(), SqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => conn.Execute(s));
            }
            // sqlite check column exists: http://stackoverflow.com/questions/18920136/check-if-a-column-exists-in-sqlite
            // sqlite alter table https://www.sqlite.org/lang_altertable.html
        }

        private void DeleteTable(string databaseName, string tableName)
        {
            if (!TableExists(databaseName, tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} drop table [{conn.Database}]");
                var sql = string.Format(@"{0} DROP TABLE {1}", SqlBuilder.BuildUseDatabase(databaseName), tableName);
                conn.Execute(sql);
            }
        }

        private void DeleteIndex(string databaseName, string tableName)
        {
            if (!TableExists(databaseName, tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                if (Options.EnableLogging)
                    Log.Debug($"{tableName} drop table [{conn.Database}]");
                var sql = IndexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    {0}
    IF EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{2}{3}' AND Object_ID = Object_ID(N'{1}'))
    BEGIN
        DROP INDEX {1}.[IX_{2}{3}]
        ALTER TABLE {1} DROP COLUMN [{2}{3}]
    END ", SqlBuilder.BuildUseDatabase(databaseName), tableName, i.Name.ToLower(), SqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => conn.Execute(s));
            }
        }

        private void EnsureOpenConnection(IDbConnection conn, IStorageOptions options)
        {
            try
            {
                conn.Open();
            }
            catch (SqlException e) // cannot login (catalog does not exist?), try without catalog in the conn string
            {
                var builder = new SqlConnectionStringBuilder(options.ConnectionString);
                if (builder.InitialCatalog.IsNullOrEmpty()) throw;
                if (options.EnableLogging)
                    Log.Warn($"fallback to db connectionstring with an empty initial catalog: {e.Message}");
                builder.InitialCatalog = "";
                options.ConnectionString = builder.ConnectionString;
                conn.ConnectionString = options.ConnectionString;
                conn.Open();
            }
        }
    }
}
