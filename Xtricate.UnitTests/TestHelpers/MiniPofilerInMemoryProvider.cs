using System;
using StackExchange.Profiling;

namespace Xtricate.UnitTests.TestHelpers
{
    public class MiniPofilerInMemoryProvider : BaseProfilerProvider
    {
        private static MiniProfiler _current;

        [Obsolete]
        public override MiniProfiler Start(ProfileLevel level, string sessionName = null)
        {
            _current = new MiniProfiler(sessionName);
            return _current;
        }

        public override void Stop(bool discardResults)
        {
            StopProfiler(_current);
        }

        public override MiniProfiler GetCurrentProfiler()
        {
            return _current;
        }

        public override MiniProfiler Start(string sessionName = null)
        {
            _current = new MiniProfiler(sessionName);
            return _current;
        }
    }
}