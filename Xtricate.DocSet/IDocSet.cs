using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface IDocSet<TDoc, TKey>
    {
        long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IEnumerable<TDoc> Load(TKey key, IEnumerable<string> tags = null, int skip = 0, int take = 0);
        IEnumerable<TDoc> Load(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criteria = null, int skip = 0, int take = 0);
        TDoc Store(TDoc document, IEnumerable<string> tags = null);
        IEnumerable<TDoc> Store(IEnumerable<TDoc> documents, IEnumerable<string> tags = null);
        //void Delete(IEnumerable<TDoc> documents, IEnumerable<string> tags = null);
        void Delete(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        void Delete(TDoc document);
        void Delete(TKey key, IEnumerable<string> tags = null);
    }
}