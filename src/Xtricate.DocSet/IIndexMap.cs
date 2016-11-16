using System;
using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface IIndexMap<T>
    {
        string Description { get; set; }

        string Name { get; set; }

        Func<T, object> Value { get; set; }

        Func<T, IEnumerable<object>> Values { get; set; }
    }
}