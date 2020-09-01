using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.SettingsReader.ReloadingManager;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.AzureTable.Sinks.AzureTable
{
    public class AzureTableSink : PeriodicBatchingSink
    {
        private readonly INoSQLTableStorage<LogEntity> _storage;

        public AzureTableSink(AzureTableSinkOptions options)
            : base(options.BatchSizeLimit, options.Period)
        {
            _storage = AzureTableStorage<LogEntity>.Create(
                ConstantReloadingManager.From(options.ConnectionString),
                options.TableName,
                new ConsoleLogFactory());
        }

        protected override Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var groups = events
                .Select(e => LogEntity.FromLogEvent(e))
                .GroupBy(e => e.PartitionKey);

            var tasks = new List<Task>();

            foreach(var group in groups)
            {
                tasks.Add(
                    _storage.InsertBatchAndGenerateRowKeyAsync(
                        group.ToArray(),
                        (entity, retryNum, batchItemNum) => LogEntity.GenerateRowKey(entity.DateTime, batchItemNum, retryNum)));
            }

            return Task.WhenAll(tasks);
        }
    }
}
