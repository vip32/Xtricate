using System.Collections.Generic;

namespace XtricateSql
{
    public interface IDocumentContext<T>
    {
        IEnumerable<T> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IEnumerable<T> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IEnumerable<T> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        T Store(T entity, IEnumerable<string> tags = null);
        IEnumerable<T> Store(IEnumerable<T> entities, IEnumerable<string> tags = null);
        void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities, IEnumerable<string> tags = null);
        void DeleteAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
    }
}