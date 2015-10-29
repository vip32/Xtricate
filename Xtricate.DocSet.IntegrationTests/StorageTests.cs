using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using StackExchange.Profiling;
using Xtricate.DocSet.IntegrationTests.Profiling;

namespace Xtricate.DocSet.IntegrationTests
{
    [TestFixture]
    public class StorageTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            MiniProfiler.Settings.Storage = new MiniPofilerInMemoryStorage();
            MiniProfiler.Settings.ProfilerProvider = new MiniPofilerInMemoryProvider();
        }

        [TestCase]
        [TestCase]
        public void UpsertTest()
        {
            int count;
            var options = new StorageOptions("TestDb", "StorageTests");
            var connectionFactory = new SqlConnectionFactory();
            var indexMap = new List<IDocIndexMap<TestDocument>>
            {
                new DocIndexMap<TestDocument>(nameof(TestDocument.Name), i => i.Name),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Group), i => i.Group),
                new DocIndexMap<TestDocument>(nameof(TestSku.Sku), values: i => i.Skus.Select(s => s.Sku)),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Date), i =>
                    i.Date.HasValue ? i.Date.Value.ToString("s") : null)
            };
            var storage = new Storage<TestDocument>(connectionFactory, options,
                new JsonNetSerializer(), new Hasher(), indexMap);

            MiniProfiler.Start();
            var mp = MiniProfiler.Current;

            using (mp.Step("initialize"))
            {
                storage.Initialize();
            }
            using (mp.Step("initial count"))
            {
                count = storage.Count();
            }

            using (mp.Step("upsert fixed key"))
            {
                var result = storage.Upsert("key1", new Fixture().Create<TestDocument>(), new[] { "en-US" });
                Assert.That(result, Is.EqualTo(StorageAction.Updated));
            }
            using (mp.Step("upsert string"))
            {
                var result = storage.Upsert(Guid.NewGuid(), new Fixture().Create<TestDocument>(), new[] { "en-US" });
                Assert.That(result, Is.EqualTo(StorageAction.Inserted));
            }
            using (mp.Step("upsert int"))
            {
                var result = storage.Upsert(DateTime.Now.Epoch() + count, new Fixture().Create<TestDocument>(), new[] { "en-US" });
                Assert.That(result, Is.EqualTo(StorageAction.Inserted));
            }
            using (mp.Step("upsert string"))
            {
                var result = storage.Upsert(Guid.NewGuid(), new Fixture().Create<TestDocument>(), new[] { "en-US" });
                Assert.That(result, Is.EqualTo(StorageAction.Inserted));
            }
            using (mp.Step("upsert int"))
            {
                var result = storage.Upsert(DateTime.Now.Epoch() + 1 + count, new Fixture().Create<TestDocument>(), new[] { "en-US" });
                Assert.That(result, Is.EqualTo(StorageAction.Inserted));
            }

            using (mp.Step("post count"))
            {
                Assert.That(storage.Count(), Is.EqualTo(count + 4));
            }
            Trace.WriteLine($"trace: {mp.RenderPlainText()}");
            MiniProfiler.Stop();
        }

        [Test]
        public void InitializeTest()
        {
            var options = new StorageOptions("TestDb", "StorageTests");
            var connectionFactory = new SqlConnectionFactory();
            var indexMap = new List<IDocIndexMap<TestDocument>>
            {
                new DocIndexMap<TestDocument>(nameof(TestDocument.Name), i => i.Name),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Group), i => i.Group),
                new DocIndexMap<TestDocument>(nameof(TestSku.Sku), values: i => i.Skus.Select(s => s.Sku)),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Date), i =>
                    i.Date.HasValue ? i.Date.Value.ToString("s") : null)
            };
            var storage = new Storage<TestDocument>(connectionFactory, options,
                new JsonNetSerializer(), new Hasher(), indexMap);

            storage.Initialize();
            storage.Reset();

            Assert.That(storage.Count(), Is.EqualTo(0));
        }
    }

    public class TestDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public TestEnum State { get; set; }
        public DateTime? Date { get; set; }
        public IEnumerable<TestSku> Skus { get; set; }
    }

    public class TestSku
    {
        public string Sku { get; set; }
    }

    public enum TestEnum
    {
        Open,
        Closed
    }
}