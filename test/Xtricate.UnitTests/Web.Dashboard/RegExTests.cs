using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xtricate.UnitTests.Web.Dashboard
{
    [TestFixture]
    public class RegExTests
    {
        [Test]
        public void TsScriptRegex()
        {
            // A
            // The input string we are using
            var input = "http://test.com/dasboard/ts/script.ts";
            //var input = "one two three";
            Trace.WriteLine("input: " + input);
            // B
            // The regular expression we use to match
            //var r1 = new Regex(@"/ts/\S+");
            var r1 = new Regex(@"/ts/(?<ScriptName>\S+)");

            //"/ts/(?<ScriptName>\\w+\\.\\w+)"
            //var r1 = new Regex(@"two");

            // C
            // Match the input and write results
            var match = r1.Match(input);
            if (match.Success)
            {
                var v = match.Groups["ScriptName"].Value;
                Trace.WriteLine(string.Format("group0: {0}", v));
            }
            else
            {
                Trace.WriteLine("no match");
            }
        }
    }
}
