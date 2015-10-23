using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace XtricateSql
{
    public class DocumentIndexContext<T> : IDocumentContext<T>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IEnumerable<IDocumentIndexMap<T>> _indexMap;
        private readonly IDocumentSchema _schema;
        private readonly ISerializer _serialize;

        public DocumentIndexContext(IDocumentSchema schema, ISerializer serializer,
            IDbConnectionFactory connectionFactory, IEnumerable<IDocumentIndexMap<T>> indexMap)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));
            if (indexMap == null) throw new ArgumentNullException(nameof(indexMap));

            _schema = schema;
            _serialize = serializer;
            _connectionFactory = connectionFactory;
            _indexMap = indexMap;
        }

        public IEnumerable<T> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public T Store(T entity, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Store(IEnumerable<T> entities, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<T> entities, IEnumerable<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }
    }
}