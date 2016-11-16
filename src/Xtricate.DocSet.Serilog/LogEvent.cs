using System;
using System.Collections.Generic;

namespace Xtricate.DocSet.Serilog
{
    public class LogEvent
    {
        public string Key { get; set; }

        public string CorrelationId { get; set; }

        public string Level { get; set; }

        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}