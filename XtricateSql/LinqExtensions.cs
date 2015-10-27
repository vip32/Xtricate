using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtricateSql
{
    public static class LinqExtensions
    {
        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source == null || !source.Any();
        }

        public static bool IsNullOrEmpty<TSource>(this ICollection<TSource> source)
        {
            return source == null || !source.Any();
        }

        public static IEnumerable<TSource> NullToEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items.IsNullOrEmpty()) return items;
            var itemsArray = items as T[] ?? items.ToArray();
            foreach (var value in itemsArray.NullToEmpty())
            {
                if (action != null) action(value);
            }
            return itemsArray;
        }

        public static string ToString<T>(this IEnumerable<T> items, string separator)
        {
            if (items.IsNullOrEmpty()) return null;
            var sb = new StringBuilder();
            foreach (var obj in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(obj);
            }
            return sb.ToString();
        }
    }
}
