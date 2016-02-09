namespace Xtricate.DocSet
{
    public interface IStorageOptions
    {
        string ConnectionString { get; set; }
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
    }
}