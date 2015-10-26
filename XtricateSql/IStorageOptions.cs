namespace XtricateSql
{
    public interface IStorageOptions
    {
        string ConnectionString { get; set; }
        string SchemaName { get; set; }
        string TableName { get; set; }
        string TableNamePrefix { get; set; }
        string TableNameSuffix { get; set; }
    }
}