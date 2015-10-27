using System.Data.Entity.Infrastructure;
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
            var storage = new Storage<TestDocument>(connectionFactory, options);

            storage.InitializeSchema();
        }

        public void InsertTest()
        {
            var options = new StorageOptions("TestDb");
            var connectionFactory = new SqlConnectionFactory();
            var storage = new Storage<TestDocument>(connectionFactory, options);

            storage.InitializeSchema();

            var fixture = new Fixture().Customize(new MultipleCustomization());
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var doc = fixture.Create<TestDocument>();
            var command = storage.UpsertCommand(doc.Id, doc);

            Assert.That(command, Is.Not.Null);
            Assert.That(command, Is.Not.Empty);
        }
    }

    public class TestDocument
    {
        public int Id { get; set; }
    }
}
