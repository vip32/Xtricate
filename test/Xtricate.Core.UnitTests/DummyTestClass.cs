using Xunit;

namespace Xtricate.Core.Test
{
    public class DummyTestClass
    {
        // http://www.michael-whelan.net/porting-dotnet-framework-library-to-dotnet-core/
        // http://andrewlock.net/publishing-your-first-nuget-package-with-appveyor-and-myget/

        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
    }
}