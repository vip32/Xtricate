using System;
using System.Collections.Generic;
using System.Data;

namespace XtricateSql
{
    public interface IStorage<T>
    {
        IStorageOptions Options { get; }
        void InitializeSchema();
        void Execute(Action action);
        IDbCommand UpsertCommand(object key, T document, IEnumerable<string> tags = null);
        IDbCommand UpsertCommand(IDictionary<object, T> document, IEnumerable<string> tags = null);
        IDbCommand CountCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand LoadCommand(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand LoadCommand(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null);
        IDbCommand DeleteCommand(object key, IEnumerable<string> tags = null);
        IDbCommand DeleteCommand(T document);
    }
}