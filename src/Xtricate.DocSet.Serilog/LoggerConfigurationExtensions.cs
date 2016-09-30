using System;
using System.Collections.Generic;
using System.Data.Common;
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
        ///     Adds a sink that writes log events as documents using DocSet.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionsStringName">The connectionsString name.</param>
        /// <param name="schemaName">The name of the database schema.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="propertiesAsTags">The properties as tags.</param>
        /// <param name="propertiesWhiteList">The properties filter.</param>
        /// <param name="options">The options.</param>
        /// <param name="indexMap">The index map.</param>
        /// <param name="enableDocSetLogging">if set to <c>true</c> [enable document set logging].</param>
        /// <returns>
        ///     Logger configuration, allowing configuration to continue.
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
            IEnumerable<string> propertiesAsTags = null,
            IEnumerable<string> propertiesWhiteList = null,
            IStorageOptions options = null,
            List<IIndexMap<LogEvent>> indexMap = null,
            bool enableDocSetLogging = false)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            if (connectionsStringName == null) throw new ArgumentNullException(nameof(connectionsStringName));

            if (options == null)
                options = new StorageOptions(
                    new ConnectionStrings().Get(connectionsStringName),
                    schemaName: schemaName);

            if (indexMap == null)
                indexMap = new List<IIndexMap<LogEvent>>
                {
                    new IndexMap<LogEvent>(nameof(LogEvent.Level), i => i.Level),
                    new IndexMap<LogEvent>(nameof(LogEvent.Timestamp), i => i.Timestamp.ToString("s"))
                };

            try
            {
                return loggerConfiguration.Sink(
                    new DocSetSink(
                        new DocStorage<LogEvent>(new SqlConnectionFactory(), options, new SqlBuilder(),
                            new JsonNetSerializer(), null /*new Md5Hasher()*/, indexMap),
                        batchPostingLimit,
                        period ?? DocSetSink.DefaultPeriod,
                        formatProvider,
                        propertiesAsTags ?? new[] {"CorrelationId", "App" /*, "SourceContext"*/},
                        propertiesWhiteList ??
                        new[] {/*"CorrelationId",*/ "App", "SourceContext" /*"Message", "DocSetKey"*/}),
                    restrictedToMinimumLevel);
            }
            catch (DbException) // could not connect to the db, use a null docstorage instead
            {
                return loggerConfiguration.Sink(
                    new DocSetSink(
                        null,
                        batchPostingLimit,
                        period ?? DocSetSink.DefaultPeriod,
                        formatProvider,
                        propertiesAsTags ?? new[] {"CorrelationId", "App" /*, "SourceContext"*/},
                        propertiesWhiteList ??
                        new[] {/*"CorrelationId",*/ "App", "SourceContext" /*"Message", "DocSetKey"*/}),
                    restrictedToMinimumLevel);
            }
            return null;
        }
    }
}