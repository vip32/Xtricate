using Humanizer;

namespace Xtricate.DocSet.Sqlite
{
    public class SqliteStorageOptions : StorageOptions
    {
        public SqliteStorageOptions(string connectionString, string schemaName = null, string tableName = null,
            string tableNamePrefix = null, string tableNameSuffix = null, bool useTransactions = false,
            bool bufferedLoad = false, int defaultTakeSize = 0, int maxTakeSize = 0)
            : base(connectionString, schemaName, tableName, tableNamePrefix, tableNameSuffix, useTransactions, bufferedLoad,
                defaultTakeSize, maxTakeSize)
        {
        }

        public override string GetTableName<T>(string suffix = null)
        {
            var tableName = string.IsNullOrEmpty(TableName) ? typeof(T).Name.Pluralize() : TableName;
            if (!string.IsNullOrEmpty(TableNamePrefix))
                tableName = TableNamePrefix + tableName;
            if (!string.IsNullOrEmpty(TableNameSuffix))
                tableName = tableName + TableNameSuffix;
            if (!string.IsNullOrEmpty(suffix))
                tableName = tableName + suffix;
            return !string.IsNullOrEmpty(SchemaName)
                ? $"[{SchemaName}.{tableName}]"
                : $"[{tableName}]";
        }
    }
}