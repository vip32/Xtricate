using System;
using System.Collections.Generic;

namespace XtricateSql
{
    public class DocIndexSet<T, TKey> : IDocSet<T>
    {
        private readonly IEnumerable<IDocIndexMap<T>> _indexMap;
        private readonly ISerializer _serializer;
        private readonly IStorage<T> _storage;

        public DocIndexSet(Func<T, TKey> key, IStorage<T> storage, ISerializer serializer)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _storage = storage;
            _serializer = serializer;
        }

        public IEnumerable<T> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public T Store(T document, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Store(IEnumerable<T> documents, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(T document)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<T> documents, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }
    }
}