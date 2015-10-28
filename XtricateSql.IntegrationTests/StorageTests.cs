using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace XtricateSql.IntegrationTests
{
    [TestFixture]
    public class StorageTests
    {
        public void UpsertTest()
        {
            var options = new StorageOptions("TestDb", "SS");
            var connectionFactory = new SqlConnectionFactory();
            var indexMap = new List<IDocIndexMap<TestDocument>>
            {
                new DocIndexMap<TestDocument>(nameof(TestDocument.Name), i => i.Name),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Group), i => i.Group),
                new DocIndexMap<TestDocument>(nameof(TestSku.Sku), values: i => i.Skus.Select(s => s.Sku)),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Date), i =>
                    i.Date.HasValue ? i.Date.Value.ToString("s") : null)
            };
            var storage = new Storage<TestDocument>(connectionFactory, options, new JsonNetSerializer(), indexMap);

            storage.Initialize();
            var count = storage.Count();

            var document = new Fixture().Create<TestDocument>();
            var result = storage.Upsert(document.Id, document, new[] {"en-US"});

            Assert.That(result, Is.EqualTo(StorageAction.Inserted));
            Assert.That(storage.Count(), Is.EqualTo(count + 1));
        }

        [Test]
        public void InitializeTest()
        {
            var options = new StorageOptions("TestDb", "SS");
            var connectionFactory = new SqlConnectionFactory();
            var indexMap = new List<IDocIndexMap<TestDocument>>
            {
                new DocIndexMap<TestDocument>(nameof(TestDocument.Name), i => i.Name),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Group), i => i.Group),
                new DocIndexMap<TestDocument>(nameof(TestSku.Sku), values: i => i.Skus.Select(s => s.Sku)),
                new DocIndexMap<TestDocument>(nameof(TestDocument.Date), i =>
                    i.Date.HasValue ? i.Date.Value.ToString("s") : null)
            };
            var storage = new Storage<TestDocument>(connectionFactory, options, new JsonNetSerializer(), indexMap);

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