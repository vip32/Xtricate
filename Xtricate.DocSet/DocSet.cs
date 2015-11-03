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
        private readonly IEnumerable<Func<TDoc, string>> _tagMap;

        public DocSet(Func<TDoc, TKey> key, IStorage<TDoc> storage,
            IEnumerable<Func<TDoc, string>> tagMap = null, IDocSet<TDoc> docIndexSet = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            _key = key;
            _storage = storage;
            _tagMap = tagMap;
            _docIndexSet = docIndexSet;

            _storage.Initialize();
        }

        public int Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            return _storage.Count(tags, criteria);
        }

        public IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return null;
            return _storage.Load(key, tags, criteria);
        }

        public IEnumerable<TDoc> Load(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            return _storage.Load(tags, criteria);
        }

        public IEnumerable<TDoc> Store(IEnumerable<TDoc> documents, IEnumerable<string> tags = null)
        {
            if (documents == null) return null;
            foreach (var document in documents)
                Store(document, tags);
            return documents;
        }

        public TDoc Store(TDoc document, IEnumerable<string> tags = null)
        {
            if (document == null) return default(TDoc);
            _storage.Upsert(_key(document), document, EnsureTags(document, tags, _tagMap));
            return document;
        }

        public void Delete(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            foreach (var document in Load(tags, criteria))
                Delete(_key(document), tags, criteria);
        }

        public void Delete(TDoc document)
        {
            if (document == null) return;
            Delete(_key(document), EnsureTags(document, null, _tagMap));
        }

        public void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return;
            _storage.Delete(key, tags, criteria);
        }

        private IEnumerable<string> EnsureTags(TDoc document,
            IEnumerable<string> tags, IEnumerable<Func<TDoc, string>> tagMap)
        {
            if ((tags == null || !tags.Any()) && (tagMap != null && tagMap.Any()))
                return tagMap.Select(tm => tm(document));
            return tags;
        }
    }
}