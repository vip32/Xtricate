using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.DocSet
{
    public class SqlBuilder : ISqlBuilder
    {
        public virtual string IndexColumnNameSuffix => "_idx";

        public virtual string BuildUseDatabase(string databaseName = null)
        {
            if (string.IsNullOrEmpty(databaseName)) return null;
            return $"USE [{databaseName}]; ";
        }

        public virtual string BuildDeleteByKey(string tableName)
        {
            return $"DELETE FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildDeleteByTags(string tableName)
        {
            return $"DELETE FROM {tableName} WHERE ";
        }

        public virtual string BuildValueSelectByKey(string tableName)
        {
            return $"SELECT [value] FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildValueSelectByTags(string tableName)
        {
            return $"SELECT [value] FROM {tableName} WHERE [id]>0";
        }

        public virtual string BuildDataSelectByKey(string tableName)
        {
            return $"SELECT [data] FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildTagSelect(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return "";
            return $" AND [tags] LIKE '%||{Sanatize(tag)}||%'";
        }

        public virtual string BuildCriteriaSelect<TDoc>(
            IEnumerable<IIndexMap<TDoc>> indexMaps = null,
            ICriteria criteria = null)
        {
            if (indexMaps == null || !indexMaps.Any()) return null;
            if (criteria == null) return null;

            var indexMap = indexMaps.FirstOrDefault(i =>
                i.Name.Equals(criteria.Name, StringComparison.OrdinalIgnoreCase));
            if (indexMap == null) return null;

            // small equals hack to handle multiple values and optimize for single values (%)
            if ((indexMap.Values != null && indexMap.Value == null) && criteria.Operator == CriteriaOperator.Eq)
                criteria.Operator = CriteriaOperator.Eqm;

            return BuildCriteriaSelect(indexMap.Name, criteria.Operator, criteria.Value);
        }

        public virtual string BuildCriteriaSelect(string column, CriteriaOperator op, string value)
        {
            if (string.IsNullOrEmpty(column)) return null;

            // TODO: use sql cmd paramaters for the values
            if (op == CriteriaOperator.Gt /*op.Equals(CriteriaOperator.Gt)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] > '||{Sanatize(value)}' ";
            if (op == CriteriaOperator.Ge /*op.Equals(CriteriaOperator.Ge)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] >= '||{Sanatize(value)}' ";
            if (op == CriteriaOperator.Lt /*op.Equals(CriteriaOperator.Lt)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] < '||{Sanatize(value)}' ";
            if (op == CriteriaOperator.Le /*op.Equals(CriteriaOperator.Le)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] <= '||{Sanatize(value)}' ";
            if (op == CriteriaOperator.Contains /*op.Equals(CriteriaOperator.Contains)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] LIKE '||%{Sanatize(value)}%||' ";
            if (op == CriteriaOperator.Eqm /*op.Equals(CriteriaOperator.Eqm)*/)
                return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] LIKE '%||{Sanatize(value)}||%' ";
                    // TODO: remove % for much faster PERF

            return $" AND [{Sanatize(column).ToLower()}{IndexColumnNameSuffix}] = '||{Sanatize(value)}||' ";
        }

        public virtual string BuildPagingSelect(int skip = 0, int take = 0,
            int defaultTakeSize = 1000, int maxTakeSize = 5000)
        {
            if (skip <= 0 && take <= 0)
                return $" OFFSET {skip.ToString()} ROWS FETCH NEXT {defaultTakeSize.ToString()} ROWS ONLY; ";
            if (skip <= 0) skip = 0;
            if (take <= 0) take = defaultTakeSize;
            if (take > maxTakeSize) take = maxTakeSize;
            return $" OFFSET {skip.ToString()} ROWS FETCH NEXT {take.ToString()} ROWS ONLY; ";
        }

        public virtual string BuildSortingSelect(SortColumn sorting = SortColumn.Id)
        {
            if (sorting == SortColumn.IdDescending)
                return " ORDER BY [id] DESC ";
            if (sorting == SortColumn.Key)
                return " ORDER BY [key] ";
            if (sorting == SortColumn.KeyDescending)
                return " ORDER BY [key] DESC ";
            if (sorting == SortColumn.Timestamp)
                return " ORDER BY [timestamp] ";
            if (sorting == SortColumn.TimestampDescending)
                return " ORDER BY [timestamp] DESC ";
            return " ORDER BY [id] ";
        }

        public string BuildFromTillDateTimeSelect(
            DateTime? fromDateTime = null,
            DateTime? tillDateTime = null)
        {
            var result = "";
            if (fromDateTime.HasValue)
                result = $"{result} AND [timestamp] >= '{fromDateTime.Value.ToString("s")}'";
            if (tillDateTime.HasValue)
                result = $"{result} AND [timestamp] < '{tillDateTime.Value.ToString("s")}'";
            return result;
        }

        public virtual string TableNamesSelect()
        {
            return @"
    SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS Name
    FROM INFORMATION_SCHEMA.TABLES";
        }

        private string Sanatize(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.Replace("'", ""); // character data string delimiter
            value = value.Replace(";", ""); // query delimiter
            value = value.Replace("--", ""); // comment delimiter
            value = value.Replace("/*", ""); // comment delimiter
            value = value.Replace("*/", ""); // comment delimiter
            value = value.Replace("xp_", ""); // comment delimiter
            return value;
        }
    }
}