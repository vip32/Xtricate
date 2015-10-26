using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using Dapper;
using Humanizer;

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
            using (var conn = _connectionFactory.CreateConnection(Options.ConnectionString))
            {
                conn.Open();

                var schemaSql = "CREATE SCHEMA " + FullSchemaName;
                var tableSql = "CREATE TABLE " + FullTableName;
                var result1 = conn.Execute(schemaSql);
                var result2 = conn.Execute(tableSql);
                throw new NotImplementedException();
            }
        }

        public IDbCommand UpsertCommand(object key, T document, IEnumerable<string> tags = null)
        {
            // use _key(document) to get KEY
            throw new NotImplementedException();
        }

        public IDbCommand UpsertCommand(IDictionary<object, T> document, IEnumerable<string> tags = null)
        {
            // use _key(document) to get KEY
            throw new NotImplementedException();
        }

        public IDbCommand LoadCommand(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IDbCommand LoadCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IDbCommand DeleteCommand(object key, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public IDbCommand DeleteCommand(T document)
        {
            throw new NotImplementedException();
        }

        private string FullSchemaName
        {
            get { return Options.SchemaName; }
        }

        private string FullTableName
        {
            get
            {
                var result = string.IsNullOrEmpty(Options.TableName) ? typeof(T).Name.Pluralize() : Options.TableName;
                if (string.IsNullOrEmpty(Options.TableNamePrefix))
                    result += Options.TableNamePrefix + result;
                if (string.IsNullOrEmpty(Options.TableNameSuffix))
                    result += result + Options.TableNameSuffix;
                return !string.IsNullOrEmpty(Options.SchemaName)
                    ? string.Format("[{0}].[{1}]", Options.SchemaName, result)
                    : result;
            }
        }
    }
}