using System;
using Humanizer;

namespace Xtricate.DocSet
{
    public class StorageOptions : IStorageOptions
    {
        public StorageOptions(string connectionString, string schemaName = null, string tableName = null,
            string tableNamePrefix = null, string tableNameSuffix = null, bool useTransactions = false,
            bool bufferedLoad = false, int defaultTakeSize = 1000, int maxTakeSize = 1000)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException(nameof(connectionString));

            ConnectionString = connectionString;
            SchemaName = schemaName;
            TableName = tableName;
            TableNamePrefix = tableNamePrefix;
            TableNameSuffix = tableNameSuffix;
            UseTransactions = useTransactions;
            BufferedLoad = bufferedLoad;
            DefaultTakeSize = defaultTakeSize > maxTakeSize ? maxTakeSize : defaultTakeSize;
            MaxTakeSize = maxTakeSize;
        }

        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public bool BufferedLoad { get; set; }
        public string TableName { get; set; }
        public string TableNamePrefix { get; set; }
        public string TableNameSuffix { get; set; }
        public bool UseTransactions { get; set; }
        public int DefaultTakeSize { get; set; }
        public int MaxTakeSize { get; set; }

        public virtual string GetTableName<T>(string suffix = null)
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
    }
}