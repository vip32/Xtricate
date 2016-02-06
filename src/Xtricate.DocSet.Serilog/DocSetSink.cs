using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Xtricate.DocSet.Serilog
{
    public class DocSetSink : PeriodicBatchingSink
    {
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);
        private readonly IFormatProvider _formatProvider;
        private readonly IStorage<LogEvent> _storage;
        private readonly IEnumerable<string> _propertiesAsTags;

        public DocSetSink(IStorage<LogEvent> storage, int batchSizeLimit, TimeSpan period,
            IFormatProvider formatProvider, IEnumerable<string>  propertiesAsTags = null)
            : base(batchSizeLimit, period)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            _storage = storage;
            _formatProvider = formatProvider;
            _propertiesAsTags = propertiesAsTags;
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events.NullToEmpty())
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty("Message",
                    new ScalarValue(logEvent.RenderMessage(_formatProvider))));
                IEnumerable<string> tags = null;
                if(!logEvent.Properties.IsNullOrEmpty() && !_propertiesAsTags.IsNullOrEmpty())
                    tags = logEvent.Properties.Where(p => _propertiesAsTags.Contains(p.Key)).Select(p => p.Value.ToString());
                _storage.Upsert(Guid.NewGuid().ToString().Replace("-", "").ToUpper(), logEvent, tags, forceInsert: true);
            }
        }
    }
}