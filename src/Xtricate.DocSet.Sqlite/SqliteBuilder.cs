namespace Xtricate.DocSet.Sqlite
{
    public class SqliteBuilder : SqlBuilder
    {
        public override string BuildPagingSelect(int skip = 0, int take = 0, int defaultTakeSize = 1000, int maxTakeSize = 5000)
        {
            if (skip <= 0 && take <= 0) return $" ORDER BY [KEY] LIMIT {skip},{defaultTakeSize}";
            if (skip <= 0) skip = 0;
            if (take <= 0) take = defaultTakeSize;
            if (take > maxTakeSize) take = maxTakeSize;
            return $" ORDER BY [KEY] LIMIT {skip},{take}";
        }

        public override string TableNamesSelect()
        {
            return @"SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
        }
    }
}