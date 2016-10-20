using System.Collections.Generic;
using System.Linq;

namespace Xtricate.Core.Templ
{
    public static partial class Extensions
    {
        /// <summary>
        ///     Converts an null list to an empty list. avoids null ref exceptions
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> NullToEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }
    }
}