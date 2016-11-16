using System;
using System.Collections.Generic;
using System.IO;

namespace Xtricate.DocSet
{
    public interface IDocStorage<TDoc>
    {
        void Reset(bool indexOnly = false);

        void Execute(Action action);

        bool Exists(object key, IEnumerable<string> tags = null);

        StorageAction Upsert(object key, TDoc document, IEnumerable<string> tags = null, bool forceInsert = false,
            DateTime? timestamp = null);

        StorageAction Upsert(object key, Stream data, IEnumerable<string> tags = null, bool forceInsert = false,
            DateTime? timestamp = null);

        StorageAction Upsert(object key, TDoc document, Stream data, IEnumerable<string> tags = null,
            bool forceInsert = false, DateTime? timestamp = null);

        long Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null);

        IEnumerable<object> LoadKeys(IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null);

        IEnumerable<TDoc> LoadValues(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criteria = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0);

        IEnumerable<Stream> LoadData(object key, IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0);

        IEnumerable<TDoc> LoadValues(IEnumerable<string> tags = null,
            IEnumerable<Criteria> criterias = null,
            DateTime? fromDateTime = null, DateTime? tillDateTime = null,
            int skip = 0, int take = 0);

        StorageAction Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criterias = null);

        StorageAction Delete(IEnumerable<string> tags, IEnumerable<Criteria> criterias = null);
    }
}