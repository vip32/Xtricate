﻿using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Xtricate.DocSet.Serilog
{
    public class DocSetSink : ILogEventSink
    {
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);
        private readonly IFormatProvider _formatProvider;
        private readonly IDocStorage<LogEvent> _storage;
        private readonly IEnumerable<string> _propertiesAsTags;
        private readonly IEnumerable<string> _propertiesWhiteList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocSetSink"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="batchSizeLimit">The batch size limit.</param>
        /// <param name="period">The period.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <param name="propertiesAsTags">The properties as tags.</param>
        /// <param name="propertiesWhiteList">The properties filer.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public DocSetSink(IDocStorage<LogEvent> storage, int batchSizeLimit, TimeSpan period,
            IFormatProvider formatProvider, IEnumerable<string> propertiesAsTags = null,
            IEnumerable<string> propertiesWhiteList = null)
        {
            //if (storage == null) throw new ArgumentNullException(nameof(storage));

            _storage = storage;
            _formatProvider = formatProvider;
            _propertiesAsTags = propertiesAsTags;
            _propertiesWhiteList = propertiesWhiteList;
        }

        public void Emit(global::Serilog.Events.LogEvent logEvent)
        {
            if (_storage == null) return;
            if (logEvent == null) return;
            var key = Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper();

            //UpdateProperties(logEvent, key);
            //FilterProperties(logEvent);

            _storage.Upsert(key, new LogEvent
            {
                Key = key,
                CorrelationId = GetCorrelationId(logEvent),
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(_formatProvider),
                Timestamp = logEvent.Timestamp.DateTime,
                Properties = GetProperties(logEvent)
            }, GetTags(logEvent), forceInsert: true, timestamp: logEvent.Timestamp.DateTime);
        }

        private static string GetCorrelationId(global::Serilog.Events.LogEvent logEvent)
        {
            LogEventPropertyValue correlationProp;
            logEvent.Properties.TryGetValue("CorrelationId", out correlationProp);
            string correlationId = null;
            if (correlationProp != null)
                correlationId = correlationProp.ToString().Trim('"');
            return correlationId;
        }

        //private void UpdateProperties(global::Serilog.Events.LogEvent logEvent, string key)
        //{
        //    logEvent.AddOrUpdateProperty(new LogEventProperty("Message",
        //        new ScalarValue(logEvent.RenderMessage(_formatProvider))));
        //    logEvent.AddOrUpdateProperty(new LogEventProperty("DocSetKey",
        //        new ScalarValue(key)));
        //}

        private IEnumerable<string> GetTags(global::Serilog.Events.LogEvent logEvent)
        {
            if (logEvent.Properties.IsNullOrEmpty() || _propertiesAsTags.IsNullOrEmpty()) return null;
            return
                logEvent.Properties.Where(p => _propertiesAsTags.Contains(p.Key))
                    .Select(p => p.Value != null ? p.Value.ToString().Trim('"') : string.Empty);
        }

        //private void FilterProperties(global::Serilog.Events.LogEvent logEvent)
        //{
        //    if (_propertiesWhiteList == null || !_propertiesWhiteList.Any()) return;
        //    foreach (var prop in logEvent.Properties.Where(prop => !_propertiesWhiteList.Contains(prop.Key)))
        //    {
        //        logEvent.RemovePropertyIfPresent(prop.Key);
        //    }
        //}

        private Dictionary<string, string> GetProperties(global::Serilog.Events.LogEvent logEvent)
        {
            if (logEvent.Properties.IsNullOrEmpty()) return null;
            if(_propertiesWhiteList == null || !_propertiesWhiteList.Any())
                return logEvent.Properties.ToDictionary(p => p.Key, p => p.Value != null ? p.Value.ToString().Trim('"') : string.Empty);

            return logEvent.Properties.Where(prop => _propertiesWhiteList.Contains(prop.Key))
                .ToDictionary(prop => prop.Key, p => p.Value != null ? p.Value.ToString().Trim('"') : string.Empty);
        }
    }
}