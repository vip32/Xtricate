using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.DocSet
{
    public class DocSet<TDoc, TKey> : IDocSet<TDoc, TKey>
    {
        private readonly IDocStorage<TDoc> _docStorage;
        private readonly Func<TDoc, TKey> _key;
        private readonly IEnumerable<Func<TDoc, string>> _tagMap;

        public DocSet(Func<TDoc, TKey> key, IDocStorage<TDoc> docStorage,
            IEnumerable<Func<TDoc, string>> tagMap = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (docStorage == null) throw new ArgumentNullException(nameof(docStorage));

            _key = key;
            _docStorage = docStorage;
            _tagMap = tagMap;
        }

        public long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            return _docStorage.Count(tags, criteria);
        }

        public IEnumerable<TDoc> Load(TKey key, IEnumerable<string> tags = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            if (key == null) return null;
            return _docStorage.Load(key, tags, null, fromDateTime, tillDateTime, skip, take);
        }

        public IEnumerable<TDoc> Load(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criteria = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0)
        {
            return _docStorage.Load(tags, criteria, fromDateTime, tillDateTime, skip, take);
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
            _docStorage.Upsert(_key(document), document, EnsureTags(document, tags, _tagMap));
            return document;
        }

        public void Delete(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            foreach (var document in Load(tags, criteria))
                Delete(_key(document), tags);
        }

        public void Delete(TDoc document)
        {
            if (document == null) return;
            Delete(_key(document), EnsureTags(document, null, _tagMap));
        }

        public void Delete(TKey key, IEnumerable<string> tags = null)
        {
            if (key == null) return;
            _docStorage.Delete(key, tags);
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