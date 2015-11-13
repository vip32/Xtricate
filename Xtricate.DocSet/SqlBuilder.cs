using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.DocSet
{
    public class SqlBuilder : ISqlBuilder
    {
        public string IndexColumnNameSuffix => "_idx";

        public string BuildTagSelect(string tag)
        {
            return $" AND [tags] LIKE '%||{tag}||%'";
        }

        public string BuildCriteriaSelect<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null)
        {
            if (indexMaps == null || !indexMaps.Any()) return null;
            if (criteria == null) return null;



            var indexMap = indexMaps.FirstOrDefault(i =>
                i.Name.Equals(criteria.Name, StringComparison.InvariantCultureIgnoreCase));
            if (indexMap == null) return null;

            // small equals hack to handle multiple values and optimize for single values (%)
            if ((indexMap.Values != null && indexMap.Value == null) && criteria.Operator == CriteriaOperator.Eq)
                criteria.Operator = CriteriaOperator.Eqm;

            return BuildCriteriaSelect(indexMap.Name, criteria.Operator, criteria.Value);
        }

        public string BuildCriteriaSelect(string column, CriteriaOperator op, string value)
        {
            if (string.IsNullOrEmpty(column)) return null;

            if (op.Equals(CriteriaOperator.Gt))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] > '||{value}' ";
            if (op.Equals(CriteriaOperator.Ge))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] >= '||{value}' ";
            if (op.Equals(CriteriaOperator.Lt))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] < '||{value}' ";
            if (op.Equals(CriteriaOperator.Le))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] <= '||{value}' ";
            if (op.Equals(CriteriaOperator.Contains))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] LIKE '||%{value}%||' ";
            if (op.Equals(CriteriaOperator.Eqm))
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] LIKE '%||{value}||%' "; // TODO: remove % for much faster PERF

            return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] = '||{value}||' ";
        }
    }
}
