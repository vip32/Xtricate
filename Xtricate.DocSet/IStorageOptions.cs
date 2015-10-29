namespace Xtricate.DocSet
{
    public interface IStorageOptions
    {
        string ConnectionString { get; set; }
        string SchemaName { get; set; }
        string TableName { get; set; }
        string TableNamePrefix { get; set; }
        string TableNameSuffix { get; set; }
        bool UseTransactions { get; set; }
        string GetDocTableName<T>(string suffix = null);
        string GetIndexTableName<T>();
    }
}