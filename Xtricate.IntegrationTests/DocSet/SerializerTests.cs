using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using StackExchange.Profiling;
using Xtricate.DocSet;
using Xtricate.DocSet.Extras;
using Xtricate.UnitTests.TestHelpers;

namespace Xtricate.IntegrationTests
{
    public class SerializerTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            MiniProfiler.Settings.Storage = new MiniPofilerInMemoryStorage();
            MiniProfiler.Settings.ProfilerProvider = new MiniPofilerInMemoryProvider();
        }

        [TestCase(100, false)]
        [TestCase(100, true)]
        [TestCase(101, true)]
        public void JsonNetPerformanceTests(int docCount, bool warmup)
        {
            Trace.WriteLine($"JsonNetPerformanceTests: warmup={docCount}, count={docCount}");
            var docs = new Fixture().CreateMany<TestDocument>(docCount).ToList();
            MiniProfiler.Start();
            var mp = MiniProfiler.Current;
            Trace.WriteLine("performance test on: " + docs.Count() + " docs");

            var jilSserializer = new JilSerializer();
            Trace.WriteLine("start JIL");
            if (warmup) jilSserializer.ToJson(new Fixture().Create<TestDocument>()); // warmup
            using (mp.Step("JIL serialization"))
            {
                1.Times(i =>
                {
                    foreach (var doc in docs)
                        jilSserializer.ToJson(doc);
                });
            }

            var jsonNetSerializer = new JsonNetSerializer();
            Trace.WriteLine("start JSONNET");
            if (warmup) jsonNetSerializer.ToJson(new Fixture().Create<TestDocument>()); // warmup
            using (mp.Step("JSONNET serialization"))
            {
                1.Times(i =>
                {
                    foreach (var doc in docs)
                        jsonNetSerializer.ToJson(doc);
                });
            }

            var textSerializer = new ServiceStackTextSerializer();
            Trace.WriteLine("start JSONNET");
            if (warmup) textSerializer.ToJson(new Fixture().Create<TestDocument>()); // warmup
            using (mp.Step("SERVICESTACK serialization"))
            {
                1.Times(i =>
                {
                    foreach (var doc in docs)
                        textSerializer.ToJson(doc);
                });
            }

            Trace.WriteLine($"trace: {mp.RenderPlainText()}");
            MiniProfiler.Stop();
        }
    }
}