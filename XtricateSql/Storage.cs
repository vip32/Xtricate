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

        public Storage(IDbConnectionFactory connectionFactory, IStorageOptions options)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _connectionFactory = connectionFactory;
            Options = options;
        }

        public IStorageOptions Options { get; }

        public void InitializeSchema()
        {
            if (TableExists(Options)) return;
            using (var conn = _connectionFactory.CreateConnection(Options.ConnectionString))
            {
                conn.Open();
                Trace.WriteLine($"sql seed db: {conn.Database}, table: {Options.GetTableName<T>()}");
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
", Options.GetTableName<T>(), new Random().Next(1000, 9999));
                conn.Execute(sql);
            }
        }

        public virtual void Execute(Action action)
        {
            if (Options.UseTransactions)
                using (var transaction = _connectionFactory.CreateConnection(Options.ConnectionString).BeginTransaction())
                {
                    action();
                    transaction.Commit();
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

        private bool TableExists(IStorageOptions options)
        {
            EnsureSchema(options);

            using (var conn = _connectionFactory.CreateConnection(options.ConnectionString))
            {
                conn.Open();
                return
                    conn.Query<string>(@"
SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS Name
FROM INFORMATION_SCHEMA.TABLES")
                        .Any(t => t.Equals(options.GetTableName<T>(), StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private void EnsureSchema(IStorageOptions options)
        {
            if (string.IsNullOrEmpty(options.SchemaName)) return;
            using (var conn = _connectionFactory.CreateConnection(options.ConnectionString))
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
                    Trace.WriteLine($"create schema {e.Message}: ");
                }
            }
        }
    }
}