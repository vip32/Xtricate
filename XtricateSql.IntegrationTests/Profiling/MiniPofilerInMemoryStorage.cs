using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace XtricateSql.IntegrationTests
{
    public class MiniPofilerInMemoryStorage : IStorage
    {
        private const int Limit = 50;
        private static readonly ConcurrentBag<MiniProfiler> Bag = new ConcurrentBag<MiniProfiler>();

        public IEnumerable<Guid> List(int maxResults, DateTime? start = null, DateTime? finish = null,
            ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            return Bag.Select(p => p.Id);
        }

        public void Save(MiniProfiler profiler)
        {
            if (Bag.Count() > Limit) Reset();
            if (Bag.All(p => p.Id != profiler.Id))
                Bag.Add(profiler);
        }

        public MiniProfiler Load(Guid id)
        {
            return Bag.FirstOrDefault(p => p.Id == id);
        }

        public void SetUnviewed(string user, Guid id)
        {
        }

        public void SetViewed(string user, Guid id)
        {
        }

        public List<Guid> GetUnviewedIds(string user)
        {
            return null;
        }

        public void Reset()
        {
            Bag.Take(Bag.Count());
        }
    }
}