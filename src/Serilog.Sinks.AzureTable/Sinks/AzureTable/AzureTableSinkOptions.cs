using System;

namespace Serilog.Sinks.AzureTable.Sinks.AzureTable
{
    /// <summary>
    /// Container for all AzureTable sink configurations.
    /// </summary>
    public class AzureTableSinkOptions
    {
        /// <summary>
        /// Gets the maximum number of events to post in a single batch. Default is 500.
        /// </summary>
        public int BatchSizeLimit { get; set; } = 500;

        /// <summary>
        /// Gets the time to wait between checking for event batches. Default is 5 seconds.
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Connection string to azure storage account.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Logs table name
        /// </summary>
        public string TableName { get; set; }
    }
}
