using System.Collections.Generic;
using NUnit.Framework;

namespace Xtricate.Playground
{
    [TestFixture]
    public class EnumerableTest
    {
        [Test]
        public void Test1()
        {
            Goo();
        }

        private void Foo(IEnumerable<string> sList)
        {
            foreach (var s in sList)
            {
            }
        }

        public void Goo()
        {
            var list = new List<string>();
            for (int i = 0; i < 1000; i++)
                Foo(list);
        }
    }
}