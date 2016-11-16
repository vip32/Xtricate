using NUnit.Framework;
using Xtricate.DocSet;

namespace Xtricate.UnitTests.Dynamic
{
    [TestFixture]
    public class ExpandoTests : AssertionHelper
    {
        [Test]
        public void SafeSubstringTest()
        {
            string sut = "1234567";
            string sut2 = null;

            Assert.AreEqual(sut.SafeSubstring(0, 4), "1234");
            Assert.AreEqual(sut.SafeSubstring(0, 100), "1234567");
            Assert.AreEqual(sut.SafeSubstring(10, 12), "");
            Assert.AreEqual(sut.SafeSubstring(4, 1), "5");
            Assert.AreEqual(sut.SafeSubstring(3, 0), "");
            Assert.AreEqual(sut.SafeSubstring(3, 1), "4");
            Assert.AreEqual(sut2.SafeSubstring(3, 1), null);
            Assert.AreEqual(sut2.SafeSubstring(0, 100), null);
        }
    }
}