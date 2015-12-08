namespace Xtricate.DocSet.Sqlite
{
    public class SqliteBuilder : Xtricate.DocSet.SqlBuilder
    {
        public SqliteBuilder(IStorageOptions options) : base(options)
        {
        }

        public override string BuildPagingSelect(int skip = 0, int take = 0)
        {
            //TODO: use the _options.DefaultTakeSize & _options.MaxTakeSize
            return "";
        }

        public override string TableNamesSelect()
        {
            return @"SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
        }
    }
}