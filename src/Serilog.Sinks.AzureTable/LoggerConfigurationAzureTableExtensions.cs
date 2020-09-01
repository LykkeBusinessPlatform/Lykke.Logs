using System;
using Serilog.Configuration;
using Serilog.Sinks.AzureTable.Sinks.AzureTable;

namespace Serilog
{
    /// <summary>
    /// Provides extension methods on <see cref="LoggerSinkConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationAzureTableExtensions
    {
        /// <summary>
        /// <see cref="LoggerSinkConfiguration"/> extension that provides configuration chaining.
        /// <example>
        ///     new LoggerConfiguration()
        ///         .MinimumLevel.Verbose()
        ///         .WriteTo.AzureTable("connectionString", "tableName")
        ///         .CreateLogger();
        /// </example>
        /// </summary>
        /// <param name="loggerSinkConfiguration">Instance of <see cref="LoggerSinkConfiguration"/> object.</param>
        /// <param name="connectionString">Azure storage connection string.</param>
        /// <param name="tableName">Logs table name.</param>
        /// <param name="batchSizeLimit">The maximum number of events to post in a single batch; defaults to 500.</param>
        /// <param name="period">The time to wait between checking for event batches; defaults to 1 sec if not
        /// provided.</param>
        /// <returns>Instance of <see cref="LoggerConfiguration"/> object.</returns>
        public static LoggerConfiguration AzureTable(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string connectionString,
            string tableName,
            int? batchSizeLimit = null,
            TimeSpan? period = null)
        {
            var azureTableSinkOptions = new AzureTableSinkOptions
            {
                ConnectionString = connectionString,
                TableName = tableName
            };
            if (batchSizeLimit.HasValue)
                azureTableSinkOptions.BatchSizeLimit = batchSizeLimit.Value;
            if (period.HasValue)
                azureTableSinkOptions.Period = period.Value;
            return loggerSinkConfiguration.AzureTable(azureTableSinkOptions);
        }

        /// <summary>
        /// <see cref="LoggerSinkConfiguration"/> extension that provides configuration chaining.
        /// </summary>
        /// <param name="loggerSinkConfiguration">Instance of <see cref="LoggerSinkConfiguration"/> object.</param>
        /// <param name="azureTableSinkOptions">AzureTable sink options object.</param>
        /// <returns>Instance of <see cref="LoggerConfiguration"/> object.</returns>
        public static LoggerConfiguration AzureTable(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            AzureTableSinkOptions azureTableSinkOptions)
        {
            if (loggerSinkConfiguration == null)
                throw new ArgumentNullException(nameof(loggerSinkConfiguration));

            if (azureTableSinkOptions == null)
                throw new ArgumentNullException(nameof(azureTableSinkOptions));

            if (string.IsNullOrWhiteSpace(azureTableSinkOptions.ConnectionString))
                throw new ArgumentNullException(nameof(azureTableSinkOptions.ConnectionString));

            if (string.IsNullOrWhiteSpace(azureTableSinkOptions.TableName))
                throw new ArgumentNullException(nameof(azureTableSinkOptions.TableName));

            return loggerSinkConfiguration.Sink(new AzureTableSink(azureTableSinkOptions));
        }
    }
}
