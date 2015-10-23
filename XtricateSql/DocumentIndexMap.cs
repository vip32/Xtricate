using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XtricateSql
{
    public class DocumentIndexMap<T> : IDocumentIndexMap<T>
    {
        public DocumentIndexMap(string name, Expression<Func<T, object>> value,
            Expression<Func<T, IEnumerable<T>>> values, string description = null)
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
        public Func<T, IEnumerable<T>> Values { get; set; }
    }
}