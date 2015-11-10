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
        private readonly IEnumerable<IIndexMap<TDoc>> _indexMap;
        private readonly IStorageOptions _options;
        private readonly ISerializer _serializer;
        private readonly IHasher _hasher;

        public DocStorage(IDbConnectionFactory connectionFactory, IStorageOptions options,
            ISerializer serializer, IHasher hasher = null, IEnumerable<IIndexMap<TDoc>> indexMap = null)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _connectionFactory = connectionFactory;
            _options = options;
            _serializer = serializer;
            _hasher = hasher;
            _indexMap = indexMap;

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
            tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");

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
    UPDATE {_options.GetDocTableName<TDoc>()}
    SET [hash]=@hash,[timestamp]=@timestamp,[value]=@value WHERE [key]=@key";
                    tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");
                    result = StorageAction.Updated;
                }
                else
                {
                    Trace.WriteLine($"document insert: key={key},tags={tags?.ToString("||")}");
                    sql =
                        $@"
    INSERT INTO {_options.GetDocTableName<TDoc>()}
    ([key],[tags],[hash],[timestamp],[value]) VALUES(@key,@tags,@hash,@timestamp,@value);";
                    result = StorageAction.Inserted;
                }
                conn.Open();
                conn.Execute(sql,
                    new
                    {
                        key = key.ToString(),
                        tags = $"||{tags.ToString("||")}||",
                        hash = _hasher?.Compute(document),
                        value = _serializer.ToJson(document),
                        timestamp = DateTime.UtcNow
                    });
                return result;
            }
        }

        public virtual int Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            Trace.WriteLine($"document count: tags={tags?.ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT COUNT(*) FROM {_options.GetDocTableName<TDoc>()} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");
                conn.Open();
                return conn.Query<int>(sql).SingleOrDefault();
            }
        }

        public virtual IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criteria = null)
        {
            Trace.WriteLine($"document load: key={key},tags={tags?.ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT [value] FROM {_options.GetDocTableName<TDoc>()} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");
                conn.Open();
                var documents = conn.Query<string>(sql, new {key}, buffered: false);
                foreach (var document in documents)
                    yield return _serializer.FromJson<TDoc>(document);
            }
        }

        public virtual IEnumerable<TDoc> Load(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            Trace.WriteLine($"document load: tags={tags?.ToString("||")}");

            using (var conn = CreateConnection())
            {
                var sql = $@"
    SELECT [value] FROM {_options.GetDocTableName<TDoc>()} WHERE [id]>0";
                tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");
                conn.Open();
                var documents = conn.Query<string>(sql, buffered: false);
                foreach (var document in documents)
                    yield return _serializer.FromJson<TDoc>(document);
            }
        }

        public virtual StorageAction Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            Trace.WriteLine($"document delete: key={key},tags={tags?.ToString("||")}");
            using (var conn = CreateConnection())
            {
                var sql = $@"
    DELETE FROM {_options.GetDocTableName<TDoc>()} WHERE [key]='{key}'";
                tags.NullToEmpty().ForEach(t => sql += $" AND [tags] LIKE '%||{t}||%'");
                conn.Open();
                var num = conn.Execute(sql, new { key });
                return num > 0 ? StorageAction.Deleted : StorageAction.None;
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
            if (_indexMap == null || !_indexMap.Any()) return;
            if (!TableExists(tableName)) EnsureTable(tableName);
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine(
                    $"seed db table=[{conn.Database}].{tableName}, index={_indexMap.NullToEmpty().Select(i => i.Name).ToString(", ")}");
                var sql = _indexMap.NullToEmpty().Select(i =>
                        string.Format(@"
    IF NOT EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}_idx' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        ALTER TABLE {0} ADD [{1}_idx] NVARCHAR(2048)
        CREATE INDEX [IX_{1}_idx] ON {0} ([{1}_idx] ASC)
    END ", tableName, i.Name.ToLower()));
                sql.ForEach(s => conn.Execute(s));
            } // sqlite check column exists: http://stackoverflow.com/questions/18920136/check-if-a-column-exists-in-sqlite
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
                var sql = _indexMap.NullToEmpty().Select(i =>
                        string.Format(@"
    IF EXISTS(SELECT * FROM sys.columns
            WHERE Name = N'{1}_idx' AND Object_ID = Object_ID(N'{0}'))
    BEGIN
        DROP INDEX {0}.[IX_{1}_idx]
        ALTER TABLE {0} DROP COLUMN [{1}_idx]
    END ", tableName, i.Name.ToLower()));
                sql.ForEach(s => conn.Execute(s));
            }
        }
    }
}