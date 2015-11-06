using System.Collections.Generic;

namespace Xtricate.DocSet
{
    public interface IDocSet<TDoc, TKey>
    {
        int Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IEnumerable<TDoc> Load(TKey key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IEnumerable<TDoc> Load(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        TDoc Store(TDoc document, IEnumerable<string> tags = null);
        IEnumerable<TDoc> Store(IEnumerable<TDoc> documents, IEnumerable<string> tags = null);
        //void Delete(IEnumerable<TDoc> documents, IEnumerable<string> tags = null);
        void Delete(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        void Delete(TDoc document);
        void Delete(TKey key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
    }
}