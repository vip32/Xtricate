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
        private readonly IDocStorage<LogEvent> _storage;
        private readonly IEnumerable<string> _propertiesAsTags;
        private readonly IEnumerable<string> _propertiesFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocSetSink"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="period">The period.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="propertiesAsTags">The properties as tags.</param>
        /// <param name="propertiesFiler">The properties filer.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public DocSetSink(IDocStorage<LogEvent> storage, int batchSizeLimit, TimeSpan period,
            IFormatProvider formatProvider, IEnumerable<string>  propertiesAsTags = null, IEnumerable<string> propertiesFiler = null)
            : base(batchSizeLimit, period)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            _storage = storage;
            _formatProvider = formatProvider;
            _propertiesAsTags = propertiesAsTags;
            _propertiesFilter = propertiesFiler;
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events.NullToEmpty())
            {
                if (logEvent == null) continue;
                var key = Guid.NewGuid().ToString().Replace("-", "").ToUpper();

                UpdateProperties(logEvent, key);
                var tags = GetTags(logEvent);
                //FilterProperties(logEvent);

                _storage.Upsert(key, logEvent, tags, forceInsert: true, timestamp: logEvent.Timestamp.DateTime);
            }
        }

        private void UpdateProperties(LogEvent logEvent, string key)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty("Message",
                new ScalarValue(logEvent.RenderMessage(_formatProvider))));
            logEvent.AddOrUpdateProperty(new LogEventProperty("DocSetKey",
                new ScalarValue(key)));
        }

        private IEnumerable<string> GetTags(LogEvent logEvent)
        {
            if (!logEvent.Properties.IsNullOrEmpty() && !_propertiesAsTags.IsNullOrEmpty())
                return logEvent.Properties.Where(p => _propertiesAsTags.Contains(p.Key)).Select(p => p.Value.ToString());
            return null;
        }

        private void FilterProperties(LogEvent logEvent)
        {
            if (_propertiesFilter == null || !_propertiesFilter.Any()) return;
            foreach (var prop in logEvent.Properties.Where(prop => !_propertiesFilter.Contains(prop.Key)))
            {
                logEvent.RemovePropertyIfPresent(prop.Key);
            }
        }
    }
}