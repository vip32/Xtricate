using System;
using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public class DocIndexSet<TDoc, TKey> : IDocSet<TDoc, TKey>
    {
        private readonly IEnumerable<IDocIndexMap<TDoc>> _indexMap;
        private readonly ISerializer _serializer;
        private readonly IStorage<TDoc> _storage;

        public DocIndexSet(Func<TDoc, TKey> key, IStorage<TDoc> storage, ISerializer serializer,
            IEnumerable<IDocIndexMap<TDoc>> indexMap = null)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _storage = storage;
            _serializer = serializer;
            _indexMap = indexMap;
        }

        public int Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TDoc> Load(TKey TKey, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TDoc> Load(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public TDoc Store(TDoc document, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TDoc> Store(IEnumerable<TDoc> documents, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(TKey key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(TDoc document)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<TDoc> documents, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }
    }
}