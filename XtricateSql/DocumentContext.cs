using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Dapper;

namespace XtricateSql
{
    public class DocumentContext<T> : IDocumentContext<T>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDocumentContext<T> _documentIndexContext;
        private readonly IDocumentSchema _schema;
        private readonly ISerializer _serialize;

        public DocumentContext(IDocumentSchema schema, ISerializer serializer,
            IDbConnectionFactory connectionFactory, IDocumentContext<T> documentIndexContext = null)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));

            _schema = schema;
            _serialize = serializer;
            _connectionFactory = connectionFactory;
            _documentIndexContext = documentIndexContext;
        }

        public IEnumerable<T> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return null;
            var storage = _schema.Storage<T>();
            var command = storage.LoadCommand(key, tags, criteria);

            using (var conn = _connectionFactory.CreateConnection(_schema.Storage<T>().ConnectionString))
            {
                conn.Open();

                //command.Connection = conn;
                var result = conn.Query(command.CommandText);
                throw new NotImplementedException();
            }
        }

        public IEnumerable<T> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            var storage = _schema.Storage<T>();
            var command = storage.LoadCommand(tags, criteria);

            using (var conn = _connectionFactory.CreateConnection(_schema.Storage<T>().ConnectionString))
            {
                conn.Open();

                //command.Connection = conn;
                var result = conn.Query(command.CommandText);
                throw new NotImplementedException();
            }
        }

        public IEnumerable<T> Store(IEnumerable<T> entities, IEnumerable<string> tags = null)
        {
            if (entities == null || !entities.Any()) return entities;
            foreach (var entity in entities)
                Store(entity, tags);
            return entities;
        }

        public T Store(T entity, IEnumerable<string> tags = null)
        {
            if (entity == null) return entity;

            throw new NotImplementedException();
        }

        public void Delete(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return;
            var entity = Load(key, tags, criteria);
            if (entity != null)
                Delete(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null) return;
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<T> entities, IEnumerable<string> tags = null)
        {
            if (entities == null || !entities.Any()) return;
            foreach (var entity in entities)
                Delete(entity);
        }

        public void DeleteAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            var entities = LoadAll(tags, criteria);
            if (entities == null || !entities.Any()) return;
            foreach (var entity in entities)
                Delete(entity);
        }
    }
}