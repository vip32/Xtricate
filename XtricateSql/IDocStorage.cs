using System.Collections.Generic;
using System.Data;

namespace XtricateSql
{
    public interface IDocStorage<T>
    {
        string ConnectionString { get; }
        string SchemaName { get; }
        string TableName { get; }
        void InitializeSchema(string name);
        IDbCommand UpsertCommand(object key, T document, IEnumerable<string> tags = null);
        IDbCommand UpsertCommand(object key, IEnumerable<T> document, IEnumerable<string> tags = null);
        IDbCommand LoadCommand(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand LoadCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand DeleteCommand(object key, IEnumerable<string> tags = null);
        IDbCommand DeleteCommand(T document);
    }
}