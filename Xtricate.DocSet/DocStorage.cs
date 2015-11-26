using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;

namespace Xtricate.DocSet
{
    public class DocStorage<TDoc> : IStorage<TDoc>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IHasher _hasher;
        private readonly IEnumerable<IIndexMap<TDoc>> _indexMaps;
        private readonly IStorageOptions _options;
        private readonly ISerializer _serializer;
        private readonly ISqlBuilder _sqlBuilder;

        public DocStorage(IDbConnectionFactory connectionFactory, IStorageOptions options, ISqlBuilder sqlBuilder,
            ISerializer serializer, IHasher hasher = null, IEnumerable<IIndexMap<TDoc>> indexMaps = null)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (sqlBuilder == null) throw new ArgumentNullException(nameof(sqlBuilder));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _connectionFactory = connectionFactory;
            _options = options;
            _sqlBuilder = sqlBuilder;
            _serializer = serializer;
            _hasher = hasher;
            _indexMaps = indexMaps.NullToEmpty().ToList().Where(im => im != null).OrderBy(i => i.Name);

            _indexMaps.ForEach(im => Trace.WriteLine($"index map: {typeof (TDoc).Name} > {im.Name} [{im.Description}]"));

            Initialize();
        }

        public IDbConnection CreateConnection() => _connectionFactory.CreateConnection(_options.ConnectionString);

        public void Initialize()
        {
            EnsureSchema(_options);
            EnsureTable(_options.GetDocTableName<TDoc>());
            EnsureIndex(_options.GetIndexTableName<TDoc>());
        }

        public virtual void Reset(bool indexOnly = false)
        {
            if (!indexOnly)
                DeleteTable(_options.GetDocTableName<TDoc>());
            DeleteIndex(_options.GetIndexTableName<TDoc>());
            Initialize();
        }

        public virtual void Execute(Action action)
        {
            if (_options.UseTransactions)
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
            //Trace.WriteLine($"document exists: key={key},tags={tags?.ToString("||")}");
            var sql = $@"
    SELECT [id] FROM {_options.GetDocTableName<TDoc>()} WHERE [key]='{key}'";
            tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));

            using (var conn = CreateConnection())
            {
                conn.Open();
                return conn.Query<int>(sql, new {key}).Any();
            }
        }

        public virtual StorageAction Upsert(object key, TDoc document, IEnumerable<string> tags = null)
        {
            // http://www.databasejournal.com/features/mssql/using-the-merge-statement-to-perform-an-upsert.html
            using (var conn = CreateConnection())
            {
                string sql;
                StorageAction result;
                if (Exists(key, tags))
                {
                    Trace.WriteLine($"document update: key={key},tags={tags?.ToString("||")}");
                    sql =
                        $@"
    UPDATE {_options.GetDocTableName<TDoc>()
                            }
    SET [hash]=@hash,[timestamp]=@timestamp,[value]=@value
        {
                            _indexMaps.NullToEmpty()
                                .Select(
                                    i =>
                                        ",[" + i.Name.ToLower() + _sqlBuilder.IndexColumnNameSuffix + "]=@" +
                                        i.Name.ToLower() + _sqlBuilder.IndexColumnNameSuffix)
                                .ToString("")}
    WHERE [key]=@key";
                    tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));
                    result = StorageAction.Updated;
                }
                else
                {
                    Trace.WriteLine($"document insert: key={key},tags={tags?.ToString("||")}");
                    sql =
                        $@"
    INSERT INTO {_options.GetDocTableName<TDoc>()
                            }
        ([key],[tags],[hash],[timestamp],[value]{
                            _indexMaps.NullToEmpty()
                                .Select(i => ",[" + i.Name.ToLower() + _sqlBuilder.IndexColumnNameSuffix + "]")
                                .ToString("")})
        VALUES(@key,@tags,@hash,@timestamp,@value{
                            _indexMaps.NullToEmpty()
                                .Select(i => ",@" + i.Name.ToLower() + _sqlBuilder.IndexColumnNameSuffix)
                                .ToString("")})";
                    result = StorageAction.Inserted;
                }

                var parameters = new DynamicParameters();
                parameters.Add("key", key.ToString());
                parameters.Add("tags", $"||{tags.ToString("||")}||");
                parameters.Add("hash", _hasher?.Compute(document));
                parameters.Add("timestamp", DateTime.UtcNow);
                parameters.Add("value", _serializer.ToJson(document));
                AddIndexParameters(document, parameters);

                conn.Open();
                conn.Execute(sql, parameters);
                return result;
            }
        }

        public virtual long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null)
        {
            Trace.WriteLine($"document count: tags={tags?.ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT COUNT(*) FROM {_options.GetDocTableName<TDoc>()} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += _sqlBuilder.BuildCriteriaSelect(_indexMaps, c));
                conn.Open();
                return conn.Query<int>(sql).SingleOrDefault();
            }
        }

        public virtual IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null)
        {
            Trace.WriteLine(
                $"document load: key={key}, tags={tags?.ToString("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT [value] FROM {_options.GetDocTableName<TDoc>()} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += _sqlBuilder.BuildCriteriaSelect(_indexMaps, c));
                conn.Open();
                var documents = conn.Query<string>(sql, new {key}, buffered: false);
                foreach (var document in documents)
                    yield return _serializer.FromJson<TDoc>(document);
            }
        }

        public virtual IEnumerable<TDoc> Load(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null)
        {
            Trace.WriteLine(
                $"document load: tags={tags?.ToString("||")}, criterias={criterias?.Select(c => c.Name + ":" + c.Value).ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT [value] FROM {_options.GetDocTableName<TDoc>()} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += _sqlBuilder.BuildCriteriaSelect(_indexMaps, c));
                conn.Open();
                var documents = conn.Query<string>(sql, buffered: false);
                foreach (var document in documents)
                    yield return _serializer.FromJson<TDoc>(document);
            }
        }

        public virtual StorageAction Delete(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null)
        {
            Trace.WriteLine($"document delete: key={key},tags={tags?.ToString("||")}");
            using (var conn = CreateConnection())
            {
                var sql = $@"
    DELETE FROM {_options.GetDocTableName<TDoc>()} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += _sqlBuilder.BuildTagSelect(t));
                criterias.NullToEmpty().ForEach(c => sql += _sqlBuilder.BuildCriteriaSelect(_indexMaps, c));
                conn.Open();
                var num = conn.Execute(sql, new {key});
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
            }
        }

        private void AddIndexParameters(TDoc document, DynamicParameters parameters)
        {
            if (_indexMaps.IsNullOrEmpty()) return;
            if (parameters == null) parameters = new DynamicParameters();
            var indexColumnValues = _indexMaps.ToDictionary(i => i.Name,
                i => i.Value != null
                    ? $"||{i.Value(document)}||"
                    : $"||{i.Values(document).ToString("||")}||");

            foreach (var item in _indexMaps)
            {
                parameters.Add(item.Name.ToLower() + _sqlBuilder.IndexColumnNameSuffix,
                    indexColumnValues.FirstOrDefault(
                        i => i.Key.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase))
                        .ValueOrDefault(i => i.Value));
            }
        }

        private bool TableExists(string tableName)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                return
                    conn.Query<string>(@"
    SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS Name
    FROM INFORMATION_SCHEMA.TABLES")
                        .Any(t => t.Equals(tableName, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private void EnsureSchema(IStorageOptions options)
        {
            if (string.IsNullOrEmpty(options.SchemaName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
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
                    Trace.WriteLine($"create schema: {e.Message}: ");
                }
            }
        }

        private void EnsureTable(string tableName)
        {
            if (TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine($"seed db table [{conn.Database}].{tableName}");
                // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                var sql = string.Format(@"
    CREATE TABLE {0}(
    [uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
    [id] INTEGER NOT NULL IDENTITY(1,1),
    [key] NVARCHAR(512) NOT NULL,
    [tags] NVARCHAR(1024) NOT NULL,
    [hash] NVARCHAR(128),
    [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    [value] TEXT);

    CREATE UNIQUE CLUSTERED INDEX [IX_id_{1}] ON {0} (id)
    CREATE INDEX [IX_key_{1}] ON {0} ([key] ASC);
    CREATE INDEX [IX_tags_{1}] ON {0} ([tags] ASC);
    CREATE INDEX [IX_hash_{1}] ON {0} ([hash] ASC);",
                    tableName, new Random().Next(1000, 9999));
                conn.Execute(sql);
            }
        }

        private void EnsureIndex(string tableName)
        {
            if (_indexMaps == null || !_indexMaps.Any()) return;
            if (!TableExists(tableName)) EnsureTable(tableName);
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine(
                    $"seed db table=[{conn.Database}].{tableName}, index={_indexMaps.NullToEmpty().Select(i => i.Name).ToString(", ")}");
                var sql = _indexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    IF NOT EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}{2}' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        ALTER TABLE {0} ADD [{1}{2}] NVARCHAR(2048)
        CREATE INDEX [IX_{1}{2}] ON {0} ([{1}{2}] ASC)
    END ", tableName, i.Name.ToLower(), _sqlBuilder.IndexColumnNameSuffix));
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
                Trace.WriteLine($"drop table [{conn.Database}].{tableName}");
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
                Trace.WriteLine($"drop table [{conn.Database}].{tableName}");
                var sql = _indexMaps.NullToEmpty().Select(i =>
                    string.Format(@"
    IF EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}{2}' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        DROP INDEX {0}.[IX_{1}{2}]
        ALTER TABLE {0} DROP COLUMN [{1}{2}]
    END ", tableName, i.Name.ToLower(), _sqlBuilder.IndexColumnNameSuffix));
                sql.ForEach(s => conn.Execute(s));
            }
        }
    }
}