namespace Xtricate.DocSet.Sqlite
{
    public class SqlBuilder : Xtricate.DocSet.SqlBuilder
    {
        public SqlBuilder(IStorageOptions options) : base(options)
        {
        }

        public override string BuildPagingSelect(int skip = 0, int take = 0)
        {
            //TODO: use the _options.DefaultTakeSize & _options.MaxTakeSize
            return "";
        }
    }
}