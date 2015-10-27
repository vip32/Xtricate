using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace XtricateSql
{
    public class DocSet<T, TKey> : IDocSet<T>
    {
        private readonly Func<T, TKey> _key;
        private readonly IStorage<T> _storage;
        private readonly ISerializer _serializer;
        private readonly IDocSet<T> _docIndexSet;

        public DocSet(Func<T, TKey> key, IStorage<T> storage, ISerializer serializer, IDocSet<T> docIndexSet = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            _key = key;
            _storage = storage;
            _serializer = serializer;
            _docIndexSet = docIndexSet;

            _storage.Initialize();
        }

        public IEnumerable<T> Count(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Load(object key, IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            if (key == null) return null;
            var command = _storage.LoadCommand(key, tags, criteria);

            using (var conn = _storage.CreateConnection())
            {
                conn.Open();

                //command.Connection = conn;
                var result = conn.Query<T>(command.CommandText);
                throw new NotImplementedException();
            }
        }

        public IEnumerable<T> LoadAll(IEnumerable<string> tags = null, IEnumerable<Criteria> criteria = null)
        {
            var command = _storage.LoadCommand(tags, criteria);

            using (var conn = _storage.CreateConnection())
            {
                conn.Open();

                var result = conn.Query<T>(command.CommandText);
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
            _storage.UpsertCommand(_key(entity), entity, tags);
            return entity;
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