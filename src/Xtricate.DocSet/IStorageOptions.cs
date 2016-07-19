namespace Xtricate.DocSet
{
    public interface IStorageOptions
    {
        string ConnectionString { get; set; }
        string DataSource { get; set; }
        string DatabaseName { get; set; }
        string SchemaName { get; set; }
        bool BufferedLoad { get; set; }
        string TableName { get; set; }
        string TableNamePrefix { get; set; }
        string TableNameSuffix { get; set; }
        bool UseTransactions { get; set; }
        string GetTableName<T>(string suffix = null);
        int DefaultTakeSize { get; set; }
        int MaxTakeSize { get; set; }
        bool EnableLogging { get; set; }
        SortColumn DefaultSortColumn { get; set; }
    }

    public enum SortColumn
    {
        Id,
        IdDescending,
        Key,
        KeyDescending,
        Timestamp,
        TimestampDescending
    }
}