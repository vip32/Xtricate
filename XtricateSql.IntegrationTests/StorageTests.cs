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
        [Test]
        public void InitializeSchemaTest()
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
        }

        public void InsertTest()
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
            //storage.Reset();

            var fixture = new Fixture().Customize(new MultipleCustomization());
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var doc = fixture.Create<TestDocument>();
            var result = storage.Upsert(doc.Id, doc, new[] {"en-US"});

            Assert.That(result, Is.EqualTo(StorageAction.Inserted));
        }
    }

    public class TestDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public DateTime? Date { get; set; }
        public IEnumerable<TestSku> Skus { get; set; }
    }

    public class TestSku
    {
        public string Sku { get; set; }
    }
}
