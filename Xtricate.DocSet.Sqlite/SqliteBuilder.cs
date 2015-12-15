namespace Xtricate.DocSet.Sqlite
{
    public class SqliteBuilder : Xtricate.DocSet.SqlBuilder
    {
        public SqliteBuilder(IStorageOptions options) : base(options)
        {
        }

        public override string BuildPagingSelect(int skip = 0, int take = 0)
        {
            if (skip <= 0 && take <= 0) return $" ORDER BY [KEY] LIMIT {skip},{Options.DefaultTakeSize}";
            if (skip <= 0) skip = 0;
            if (take <= 0) take = Options.DefaultTakeSize;
            if (take > Options.MaxTakeSize) take = Options.MaxTakeSize;
            return $" ORDER BY [KEY] LIMIT {skip},{take}";
        }

        public override string TableNamesSelect()
        {
            return @"SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
        }
    }
}