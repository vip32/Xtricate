using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtricate.DocSet
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

        /// <summary>
        ///     Returns the property or a default value if the source was null
        /// </summary>
        /// <typeparam name="T1">The type of the source.</typeparam>
        /// <typeparam name="T2">The type of the property.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="property">The property to get.</param>
        /// <returns></returns>
        public static T2 ValueOrDefault<T1, T2>(this T1 source, Func<T1, T2> property)
        {
            if (typeof(T1).IsValueType)
                return Equals(source, default(T1)) ? default(T2) : property(source);
            return Equals(source, null) ? default(T2) : property(source);
        }

        /// <summary>
        ///     Returns the property or a default value if the source was null
        /// </summary>
        /// <typeparam name="T1">The type of the source.</typeparam>
        /// <typeparam name="T2">The type of the property.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="property">The property to get.</param>
        /// <param name="defaultValue">The defaule value.</param>
        /// <returns></returns>
        public static T2 ValueOrDefault<T1, T2>(this T1 source, Func<T1, T2> property, T2 defaultValue)
        {
            if (typeof(T1).IsValueType)
                return Equals(source, default(T1)) ? defaultValue : property(source);
            return Equals(source, null) ? defaultValue : property(source);
        }
    }
}