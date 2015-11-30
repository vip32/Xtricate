using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface ISqlBuilder
    {
        string IndexColumnNameSuffix { get; }
        string BuildCriteriaSelect(string column, CriteriaOperator op, string value);
        string BuildCriteriaSelect<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null);
        string BuildTagSelect(string tag);
        string BuildPagingSelect(int skip = 0, int take = 0);
    }
}