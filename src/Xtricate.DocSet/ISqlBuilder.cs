using System;
using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface ISqlBuilder
    {
        string BuildUseDatabase(string databaseName);

        string BuildDeleteByKey(string tableName);

        string BuildDeleteByTags(string tableName);

        string BuildValueSelectByKey(string tableName);

        string BuildValueSelectByTags(string tableName);

        string BuildDataSelectByKey(string tableName);

        string IndexColumnNameSuffix { get; }

        string BuildCriteriaSelect(string column, CriteriaOperator op, string value);

        string BuildCriteriaSelect<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null);

        string BuildTagSelect(string tag);

        string BuildSortingSelect(SortColumn sorting = SortColumn.Id);

        string BuildPagingSelect(int skip = 0, int take = 0, int defaultTakeSize = 0, int maxTakeSize = 0);

        string BuildFromTillDateTimeSelect(DateTime? fromDateTime = null, DateTime? tillDateTime = null);

        string TableNamesSelect();
    }
}