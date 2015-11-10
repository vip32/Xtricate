using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Xtricate.DocSet
{
    public class IndexMap<T> : IIndexMap<T>
    {
        public IndexMap(string name, Expression<Func<T, object>> value = null,
            Expression<Func<T, IEnumerable<object>>> values = null, string description = null)
        {
            Name = name;
            if (value != null)
                Value = value.Compile();
            if (values != null)
                Values = values.Compile();
            Description = description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Func<T, object> Value { get; set; }
        public Func<T, IEnumerable<object>> Values { get; set; }
    }
}