using Humanizer;

namespace Xtricate.DocSet.Sqlite
{
    public class SqliteStorageOptions : StorageOptions
    {
        public SqliteStorageOptions(string connectionString, string schemaName = null, string tableName = null,
            string tableNamePrefix = null, string tableNameSuffix = null, bool useTransactions = false,
            bool bufferedLoad = false, int defaultTakeSize = 1000, int maxTakeSize = 5000, bool enableLogging = true,
            SortColumn defaultSortColumn = SortColumn.Id)
            : base(connectionString, schemaName, tableName, tableNamePrefix, tableNameSuffix, useTransactions, bufferedLoad,
                defaultTakeSize, maxTakeSize, enableLogging, defaultSortColumn)
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