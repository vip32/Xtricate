using System;
using Humanizer;

namespace Xtricate.DocSet
{
    public class StorageOptions : IStorageOptions
    {
        public StorageOptions(string connectionString, string schemaName = null, string tableName = null,
            string tableNamePrefix = null, string tableNameSuffix = null, bool useTransactions = false)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(nameof(connectionString));

            ConnectionString = connectionString;
            SchemaName = schemaName;
            TableName = tableName;
            TableNamePrefix = tableNamePrefix;
            TableNameSuffix = tableNameSuffix;
            UseTransactions = useTransactions;
        }

        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string TableNamePrefix { get; set; }
        public string TableNameSuffix { get; set; }
        public bool UseTransactions { get; set; }

        public string GetDocTableName<T>(string suffix = null)
        {
            var tableName = string.IsNullOrEmpty(TableName) ? typeof (T).Name.Pluralize() : TableName;
            if (!string.IsNullOrEmpty(TableNamePrefix))
                tableName = TableNamePrefix + tableName;
            if (!string.IsNullOrEmpty(TableNameSuffix))
                tableName = tableName + TableNameSuffix;
            if (!string.IsNullOrEmpty(suffix))
                tableName = tableName + suffix;
            return !string.IsNullOrEmpty(SchemaName)
                ? $"[{SchemaName}].[{tableName}]"
                : $"[{tableName}]";
        }

        public string GetIndexTableName<T>()
        {
            return GetDocTableName<T>();
        }
    }
}