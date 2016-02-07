using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Xtricate.DocSet
{
    public class SqlBuilder : ISqlBuilder
    {
        protected readonly IStorageOptions Options;

        public SqlBuilder(IStorageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            Options = options;
        }
        public virtual string IndexColumnNameSuffix => "_idx";

        public virtual string BuildTagSelect(string tag)
        {
            return $" AND [tags] LIKE '%||{tag}||%'";
        }

        public virtual string BuildCriteriaSelect<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null)
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

        public virtual string BuildCriteriaSelect(string column, CriteriaOperator op, string value)
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
                return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] LIKE '%||{value}||%' ";
                    // TODO: remove % for much faster PERF

            return $" AND [{column.ToLower()}{IndexColumnNameSuffix}] = '||{value}||' ";
        }

        public virtual string BuildPagingSelect(int skip = 0, int take = 0)
        {
            if (skip <= 0 && take <= 0) return $" ORDER BY [KEY] OFFSET {skip} ROWS FETCH NEXT {Options.DefaultTakeSize} ROWS ONLY; ";
            if (skip <= 0) skip = 0;
            if (take <= 0) take = Options.DefaultTakeSize;
            if (take > Options.MaxTakeSize) take = Options.MaxTakeSize;
            return $" ORDER BY [KEY] OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY; ";
        }

        public string BuildFromTillDateTimeSelect(DateTime? fromDateTime = null, DateTime? tillDateTime = null)
        {
            var result = "";
            if (fromDateTime.HasValue)
                result += $" AND [timestamp] >= '{fromDateTime.Value.ToString("s")}'";
            if (tillDateTime.HasValue)
                result += $" AND [timestamp] < '{tillDateTime.Value.ToString("s")}'";
            return result;
        }

        public virtual string TableNamesSelect()
        {
            return @"
    SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS Name
    FROM INFORMATION_SCHEMA.TABLES";
        }
    }
}