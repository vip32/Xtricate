using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;

namespace XtricateSql
{
    public class Storage<T> : IStorage<T>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IStorageOptions _options;
        private readonly IEnumerable<IDocIndexMap<T>> _indexMap;

        public Storage(IDbConnectionFactory connectionFactory, IStorageOptions options, IEnumerable<IDocIndexMap<T>> indexMap = null)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _connectionFactory = connectionFactory;
            _options = options;
            _indexMap = indexMap;

            Initialize();
        }

        public IDbConnection CreateConnection() => _connectionFactory.CreateConnection(_options.ConnectionString);

        public void Initialize()
        {
            EnsureSchema(_options);
            EnsureDocTable();
            EnsureIndexTable();
        }

        public virtual void Reset()
        {
            DeleteTable(_options.GetDocTableName<T>());
            DeleteTable(_options.GetIndexTableName<T>());
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

        public virtual IDbCommand UpsertCommand(object key, T document, IEnumerable<string> tags = null)
        {
            // use _key(document) to get KEY
            throw new NotImplementedException();
        }

        public virtual IDbCommand UpsertCommand(IDictionary<object, T> document, IEnumerable<string> tags = null)
        {
            // use _key(document) to get KEY
            throw new NotImplementedException();
        }

        public virtual IDbCommand CountCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public virtual IDbCommand LoadCommand(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public virtual IDbCommand LoadCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public virtual IDbCommand DeleteCommand(object key, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public virtual IDbCommand DeleteCommand(T document)
        {
            throw new NotImplementedException();
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

        private void EnsureDocTable()
        {
            if (TableExists(_options.GetDocTableName<T>())) return;
            var tableName = _options.GetDocTableName<T>();
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine($"seed db table [{conn.Database}].{tableName}");
                // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
                var sql = string.Format(@"
CREATE TABLE {0}(
[uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
[id] INTEGER NOT NULL IDENTITY(1,1),
[key] NVARCHAR(2048) NOT NULL,
[tags] NVARCHAR(2048) NOT NULL,
[hash] NVARCHAR(1024) NOT NULL,
[timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
[value] TEXT);

CREATE UNIQUE CLUSTERED INDEX [IX_id_{1}] ON {0} (id)
CREATE INDEX [IX_key_{1}] ON {0} ([key] ASC);
CREATE INDEX [IX_tags_{1}] ON {0} ([tags] ASC);
CREATE INDEX [IX_hash_{1}] ON {0} ([hash] ASC);
", tableName, new Random().Next(1000, 9999));
                conn.Execute(sql);
            }
        }

        private void EnsureIndexTable()
        {
            if (_indexMap == null || !_indexMap.Any()) return;
            var tableName = _options.GetIndexTableName<T>();
            if (TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine($"seed db table=[{conn.Database}].{tableName}, index={_indexMap.NullToEmpty().Select(i => i.Name).ToString(", ")}");
                var sql = string.Format(@"
CREATE TABLE {0}(
[uid] UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL PRIMARY KEY NONCLUSTERED,
[id] INTEGER NOT NULL IDENTITY(1,1),
[key] NVARCHAR(2048) NOT NULL,
[tags] NVARCHAR(2048) NOT NULL,
[hash] NVARCHAR(1024) NOT NULL,
[timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL
{2});
CREATE UNIQUE CLUSTERED INDEX [IX_id_{1}] ON {0} (id)
CREATE INDEX [IX_key_{1}] ON {0} ([key] ASC);
CREATE INDEX [IX_tags_{1}] ON {0} ([tags] ASC);
CREATE INDEX [IX_hash_{1}] ON {0} ([hash] ASC);
{3}", tableName, new Random().Next(1000, 9999),
                    _indexMap.NullToEmpty().Select(i =>
                        string.Format(",[{0}] NVARCHAR(2048)", i.Name.ToLower())).ToString(""),
                    _indexMap.NullToEmpty().Select(i =>
                        string.Format("CREATE INDEX [ixp_{1}_{2}] ON {0} ([{1}] ASC);",
                            tableName, i.Name.ToLower(), new Random().Next(1000, 9999))).ToString(""));
                conn.Execute(sql);
            }
        }

        private void DeleteTable(string tableName)
        {
            if (!TableExists(tableName)) return;
            using (var conn = CreateConnection())
            {
                conn.Open();
                Trace.WriteLine($"drop db table [{conn.Database}].{tableName}");
                var sql = string.Format(@"DROP TABLE {0}", tableName);
                conn.Execute(sql);
            }
        }
    }
}