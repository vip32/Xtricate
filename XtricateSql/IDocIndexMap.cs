using System;
using System.Collections.Generic;

namespace XtricateSql
{
    public interface IDocIndexMap<T>
    {
        string Description { get; set; }
        string Name { get; set; }
        Func<T, object> Value { get; set; }
        Func<T, IEnumerable<T>> Values { get; set; }
    }
}