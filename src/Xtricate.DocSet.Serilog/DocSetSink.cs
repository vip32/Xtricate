using System;
using System.Collections.Generic;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Xtricate.DocSet.Serilog
{
    public class DocSetSink : PeriodicBatchingSink
    {
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);
        private readonly IStorage<LogEvent> _storage;
        readonly IFormatProvider _formatProvider;

        public DocSetSink(IStorage<LogEvent> storage, int batchSizeLimit, TimeSpan period, IFormatProvider formatProvider)
            : base(batchSizeLimit, period)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            _storage = storage;
            _formatProvider = formatProvider;
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events.NullToEmpty())
            {
                logEvent.AddOrUpdateProperty(new LogEventProperty("Message",
                    new ScalarValue(logEvent.RenderMessage(_formatProvider))));
                _storage.Upsert(Guid.NewGuid().ToString().Replace("-", "").ToUpper(), logEvent, forceInsert: true);
            }
        }
    }
}