using Humanizer;

namespace XtricateSql
{
    public class StorageOptions : IStorageOptions
    {
        public string ConnectionString { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string TableNamePrefix { get; set; }

        public string TableNameSuffix { get; set; }
    }
}