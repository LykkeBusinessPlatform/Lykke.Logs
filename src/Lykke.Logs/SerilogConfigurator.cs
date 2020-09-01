using System;
using Lykke.Common;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Lykke.Logs
{
    public static class SerilogConfigurator
    {
        public static void ConfigureSerilogConsole()
        {
            var config = new LoggerConfiguration()
                .AddFilters()
                .AddProperties()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console();

            Serilog.Log.Logger = config.CreateLogger();
        }

        public static void ConfigureSerilog(
            string azureTableConnectionString = null,
            string logsTableName = null,
            string azureQueueConnectionString = null,
            string queueName = null,
            string elasticSearchUrl = null)
        {
            var config = new LoggerConfiguration()
                .AddFilters()
                .AddProperties()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(azureTableConnectionString))
            {
                if (string.IsNullOrWhiteSpace(logsTableName))
                    throw new ArgumentNullException(nameof(logsTableName));
                config = config.WriteTo.AzureTable(azureTableConnectionString, logsTableName);
            }

            if (!string.IsNullOrWhiteSpace(azureQueueConnectionString))
            {
                if (string.IsNullOrWhiteSpace(queueName))
                    throw new ArgumentNullException(nameof(queueName));
                config = config.WriteTo.AzureQueueStorage(
                    formatter: new AzureSlackQueueFormatter(),
                    connectionString: azureQueueConnectionString,
                    storageQueueName: queueName,
                    separateQueuesByLogLevel: true);
            }

            if (!string.IsNullOrWhiteSpace(elasticSearchUrl))
                config = config.WriteTo.Elasticsearch(
                    elasticSearchUrl,
                    autoRegisterTemplate: true,
                    autoRegisterTemplateVersion: AutoRegisterTemplateVersion.ESv7);

            Serilog.Log.Logger = config.CreateLogger();
        }

        private static LoggerConfiguration AddFilters(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        }

        private static LoggerConfiguration AddProperties(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .Enrich.WithProperty("AppName", AppEnvironment.Name)
                .Enrich.WithProperty("AppVersion", AppEnvironment.Version)
                .Enrich.WithProperty("EnvInfo", AppEnvironment.EnvInfo);
        }
    }
}
