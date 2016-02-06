using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Xtricate.Configuration;

namespace Xtricate.DocSet.Serilog
{
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events as documents using DocSet.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionsStringName">The connectionsString name.</param>
        /// <param name="schemaName">The name of the database schema.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="propertiesAsTags">The properties as tags.</param>
        /// <returns>
        /// Logger configuration, allowing configuration to continue.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration DocSet(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionsStringName,
            string schemaName = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            int batchPostingLimit = 50,
            TimeSpan? period = null,
            IFormatProvider formatProvider = null,
            IEnumerable<string> propertiesAsTags = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            if (connectionsStringName == null) throw new ArgumentNullException(nameof(connectionsStringName));

            var options = new StorageOptions(new ConnectionStrings().Get(connectionsStringName), schemaName);
            var connectionFactory = new SqlConnectionFactory();
            var indexMap = new List<IIndexMap<LogEvent>>
            {
                new IndexMap<LogEvent>(nameof(LogEvent.Level), i => i.Level),
                new IndexMap<LogEvent>(nameof(LogEvent.Timestamp), i => i.Timestamp.ToString("s"))
            };
            var storage = new DocStorage<LogEvent>(connectionFactory, options, new SqlBuilder(options),
                new JsonNetSerializer(), null /*new Md5Hasher()*/, indexMap);

            var defaultedPeriod = period ?? DocSetSink.DefaultPeriod;
            return loggerConfiguration.Sink(
                new DocSetSink(
                    storage,
                    batchPostingLimit,
                    defaultedPeriod,
                    formatProvider,
                    propertiesAsTags ?? new[] {"CorrelationId", "App" /*, "SourceContext"*/ }),
                restrictedToMinimumLevel);
        }
    }
}