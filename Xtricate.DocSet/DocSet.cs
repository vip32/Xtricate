using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.DocSet
{
    public class DocSet<TDoc, TKey> : IDocSet<TDoc>
    {
        private readonly IDocSet<TDoc> _docIndexSet;
        private readonly Func<TDoc, TKey> _key;
        private readonly IStorage<TDoc> _storage;

        public DocSet(Func<TDoc, TKey> key, IStorage<TDoc> storage, IDocSet<TDoc> docIndexSet = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            _key = key;
            _storage = storage;
            _docIndexSet = docIndexSet;

            _storage.Initialize();
        }

        public IEnumerable<TDoc> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return null;
            return _storage.Load(key, tags, criteria);
        }

        public IEnumerable<TDoc> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            return _storage.Load(tags, criteria);
        }

        public IEnumerable<TDoc> Store(IEnumerable<TDoc> documents, IEnumerable<string> tags = null)
        {
            if (documents == null || !documents.Any()) return documents;
            foreach (var document in documents)
                Store(document, tags);
            return documents;
        }

        public TDoc Store(TDoc document, IEnumerable<string> tags = null)
        {
            if (document == null) return document;
            _storage.Upsert(_key(document), document, tags);
            return document;
        }

        public void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return;
            var entity = Load(key, tags, criteria);
            if (entity != null)
                Delete(entity);
        }

        public void Delete(TDoc document)
        {
            if (document == null) return;
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<TDoc> documents, IEnumerable<string> tags = null)
        {
            if (documents == null || !documents.Any()) return;
            foreach (var entity in documents)
                Delete(entity);
        }

        public void DeleteAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            var entities = LoadAll(tags, criteria);
            if (entities == null || !entities.Any()) return;
            foreach (var entity in entities)
                Delete(entity);
        }
    }
}