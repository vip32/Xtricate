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
        public void UpsertTest()
        {
            int preCount;
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
                preCount = storage.Count();
                Trace.WriteLine($"pre count: {preCount}");
            }

            var id = DateTime.Now.Epoch() + new Random().Next(10000, 99999);
            for (var i = 1; i < 0; i++)
            {
                Trace.WriteLine($"+{i}");
                using (mp.Step("upsert"))
                {
                    var result1 = storage.Upsert("key1", new Fixture().Create<TestDocument>(), new[] {"en-US"});
                //    Assert.That(result1, Is.EqualTo(StorageAction.Updated));
                //}
                //using (mp.Step("upsert string"))
                //{
                    var result2 = storage.Upsert(Guid.NewGuid(), new Fixture().Create<TestDocument>(), new[] {"en-US"});
                    //Assert.That(result2, Is.EqualTo(StorageAction.Inserted));
                //}
                //using (mp.Step("upsert int"))
                //{
                    var result3 = storage.Upsert(id + i, new Fixture().Create<TestDocument>(), new[] {"en-US"});
                    Assert.That(result3, Is.EqualTo(StorageAction.Inserted));
                }
            }
            for (var i = 1; i <= 10; i++)
            {
                using (mp.Step("load"))
                {
                    var result = storage.Load(new[] {"en-US"});
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result, Is.Not.Empty);
                    Trace.WriteLine($"loaded count: {result.Count()}");
                    //result.ForEach(r => Trace.Write(r.Id));
                    result.ForEach(r => Assert.That(r, Is.Not.Null));
                }
            }

            using (mp.Step("post count"))
            {
                var postCount = storage.Count(new[] { "en-US" });
                Trace.WriteLine($"post count: {postCount}");
                //Assert.That(storage.Count(), Is.GreaterThan(preCount));
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
        public IDictionary<string, string> Identifiers { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Group { get; set; }
        public int Position { get; set; }
        public IEnumerable<string> MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public TestEnum State { get; set; }
        public DateTime? Date { get; set; }
        public IEnumerable<TestSku> Skus { get; set; }
        public IEnumerable<TestAttributeValue> Features { get; set; }
        public IEnumerable<TestAttributeValue> Relations { get; set; }
        public IEnumerable<TestAttributeValue> Includes { get; set; }
        public IEnumerable<TestAttributeValue> Attributes { get; set; }
    }

    public class TestSku
    {
        public string Sku { get; set; }
        public string Cso { get; set; }
        public string Gtin { get; set; }
        public string Ean { get; set; }
        public string Upc { get; set; }
    }

    public class TestAttributeValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public string TextValue { get; set; }
        public int? MediaValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BooleanValue { get; set; }
        public int? CategoryValue { get; set; }
        public int? ProductValue { get; set; }
        public DateTime? DateValue { get; set; }
    }

    public enum TestEnum
    {
        Open,
        Closed
    }
}