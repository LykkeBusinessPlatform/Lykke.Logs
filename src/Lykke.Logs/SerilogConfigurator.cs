using System;
using Lykke.Common;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

namespace Lykke.Logs
{
    public class SerilogConfigurator
    {
        private LoggerConfiguration _config;
        private bool _loadedFromConfig;

        public SerilogConfigurator()
        {
            _config = new LoggerConfiguration();
        }

        public void Configure()
        {
            if (!_loadedFromConfig)
            {
                _config = _config
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console();

                AddFilters();
                AddProperties();
            }

            Serilog.Log.Logger = _config.CreateLogger();
        }

        public SerilogConfigurator AddFromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException();

            _config = _config.ReadFrom.Configuration(configuration);

            _loadedFromConfig = true;

            return this;
        }

        public SerilogConfigurator AddAzureTable(string azureTableConnectionString, string logsTableName)
        {
            if (string.IsNullOrWhiteSpace(azureTableConnectionString))
                throw new ArgumentNullException(nameof(azureTableConnectionString));
            if (string.IsNullOrWhiteSpace(logsTableName))
                throw new ArgumentNullException(nameof(logsTableName));

            _config = _config.WriteTo.AzureTable(azureTableConnectionString, logsTableName);

            return this;
        }

        public SerilogConfigurator AddAzureQueue(string azureQueueConnectionString, string queueName)
        {
            if (string.IsNullOrWhiteSpace(azureQueueConnectionString))
                throw new ArgumentNullException(nameof(azureQueueConnectionString));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            _config = _config.WriteTo.AzureQueueStorage(
                formatter: new AzureSlackQueueFormatter(),
                connectionString: azureQueueConnectionString,
                storageQueueName: queueName,
                separateQueuesByLogLevel: true);

            return this;
        }

        public SerilogConfigurator AddElasticsearch(string elasticSearchUrl)
        {
            if (string.IsNullOrWhiteSpace(elasticSearchUrl))
                throw new ArgumentNullException(nameof(elasticSearchUrl));

            _config = _config.WriteTo.Elasticsearch(
                elasticSearchUrl,
                autoRegisterTemplate: true,
                autoRegisterTemplateVersion: AutoRegisterTemplateVersion.ESv7);

            return this;
        }

        public SerilogConfigurator AddTelegram(
            string botToken,
            string chatId,
            LogEventLevel minimalLevel)
        {
            if (string.IsNullOrWhiteSpace(botToken))
                throw new ArgumentNullException(nameof(botToken));
            if (string.IsNullOrWhiteSpace(chatId))
                throw new ArgumentNullException(nameof(chatId));

            _config = _config.WriteTo.Telegram(
                botToken: botToken,
                chatId: chatId,
                restrictedToMinimumLevel: minimalLevel);

            return this;
        }

        public void AddProperties(params (string, Func<object>)[] propertiesResolvers)
        {
            for (int i = 0; i < propertiesResolvers.Length; ++i)
            {
                var (propertyName, propertyValueFunc) = propertiesResolvers[i];
                _config = _config
                    .Enrich.WithProperty(propertyName, propertyValueFunc());
            }
        }

        public void AddFilters(params (string, LogEventLevel)[] filters)
        {
            for (int i = 0; i < filters.Length; ++i)
            {
                var (source, logLevel) = filters[i];
                _config = _config
                    .MinimumLevel.Override(source, logLevel);
            }
        }

        private void AddFilters()
        {
            _config = _config
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        }

        private void AddProperties()
        {
            _config = _config
                .Enrich.WithProperty("AppName", AppEnvironment.Name)
                .Enrich.WithProperty("AppVersion", AppEnvironment.Version)
                .Enrich.WithProperty("EnvInfo", AppEnvironment.EnvInfo);
        }
    }
}
