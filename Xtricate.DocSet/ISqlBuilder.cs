using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface ISqlBuilder
    {
        string IndexColumnNameSuffix { get; }
        string BuildCriteriaSql(string column, CriteriaOperator op, string value);
        string BuildCriteriaSql<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null);
        string BuildTagSql(string tag);
    }
}