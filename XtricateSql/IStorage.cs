using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;

namespace XtricateSql
{
    public interface IStorage<T>
    {
        IStorageOptions Options { get; }
        void InitializeSchema();
        IDbCommand UpsertCommand(T document, IEnumerable<string> tags = null);
        IDbCommand UpsertCommand(IEnumerable<T> document, IEnumerable<string> tags = null);
        IDbCommand LoadCommand(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand LoadCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand DeleteCommand(object key, IEnumerable<string> tags = null);
        IDbCommand DeleteCommand(T document);
    }
}