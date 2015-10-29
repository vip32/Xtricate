using System.Linq;
using StackExchange.Profiling;

namespace Xtricate.DocSet.IntegrationTests.Profiling
{
    public static partial class Extensions
    {
        public static string RenderIndentedText(this MiniProfiler profiler)
        {
            //var miniProfiler = MiniProfiler.Current;
            var result = profiler.RenderPlainText();
            for (var i = 10; i > 0; i--)
            {
                var token = new string('>', i);
                var replacementToken = "<br/>" + string.Concat(Enumerable.Repeat("&emsp;", i));
                result = result.Replace(token, replacementToken);
            }
            return result;
        }
    }
}
