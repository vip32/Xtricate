using System;
using System.Collections.Generic;
using System.Data;

namespace Xtricate.DocSet
{
    public interface IStorage<TDoc>
    {
        IDbConnection CreateConnection();
        void Initialize();
        void Reset(bool indexOnly = false);
        void Execute(Action action);
        bool Exists(object key, IEnumerable<string> tags = null);
        StorageAction Upsert(object key, TDoc document, IEnumerable<string> tags = null);
        long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null);
        IEnumerable<TDoc> Load(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criteria = null, int skip = 0, int take = 0);
        IEnumerable<TDoc> Load(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null, int skip = 0, int take = 0);
        StorageAction Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null);
    }
}