using System;
using System.Collections.Generic;
using System.Text;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Lykke.Common;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Events;

namespace Serilog.Sinks.AzureTable.Sinks.AzureTable
{
    internal sealed class LogEntity : TableEntity
    {
        private const string ComponentPropertyName = "Component";
        private const string ProcessPropertyName = "Process";
        private const string ContextPropertyName = "Context";
        private const string MessagePropertyName = "Message";

        [UsedImplicitly]
        public DateTime DateTime { get; set; }
        [UsedImplicitly]
        public string Level { get; set; }
        [UsedImplicitly]
        public string Env { get; set; }
        [UsedImplicitly]
        public string AppName { get; set; }
        [UsedImplicitly]
        public string Version { get; set; }
        [UsedImplicitly]
        public string Component { get; set; }
        [UsedImplicitly]
        public string Process { get; set; }
        [UsedImplicitly]
        public string Context { get; set; }
        [UsedImplicitly]
        public string Type { get; set; }
        [UsedImplicitly]
        public string Stack { get; set; }
        [UsedImplicitly]
        public string Msg { get; set; }

        internal static LogEntity FromLogEvent(LogEvent logEvent)
        {
            var result = new LogEntity
            {
                PartitionKey = GeneratePartitionKey(logEvent.Timestamp.UtcDateTime),
                DateTime = logEvent.Timestamp.UtcDateTime,
                Level = GetLogLevelString(logEvent.Level, logEvent.Properties),
                Env = AppEnvironment.EnvInfo,
                AppName = AppEnvironment.Name,
                Version = AppEnvironment.Version,
                Type = logEvent.Exception?.GetType().ToString(),
                Stack = Truncate(logEvent.Exception?.ToAsyncString()),
            };

            if (logEvent.Properties.TryGetValue(ComponentPropertyName, out var componentValue))
                result.Component = GetPropertyValue(componentValue);
            if (logEvent.Properties.TryGetValue(ProcessPropertyName, out var processValue))
                result.Process = GetPropertyValue(processValue);
            if (logEvent.Properties.TryGetValue(ContextPropertyName, out var contextValue))
                result.Context = GetPropertyValue(contextValue);
            if (logEvent.Properties.TryGetValue(MessagePropertyName, out var messageValue))
                result.Msg = GetPropertyValue(messageValue);
            if (logEvent.Exception == null && string.IsNullOrWhiteSpace(result.Msg))
                result.Msg = logEvent.RenderMessage();

            return result;
        }

        internal static string GenerateRowKey(DateTime dateTime, int itemNumber, int retryNumber)
        {
            return retryNumber == 0
                ? $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}"
                : $"{dateTime:HH:mm:ss.fffffff}.{itemNumber:000}.{retryNumber:000}";
        }

        private static string GetPropertyValue(LogEventPropertyValue propertyValue)
        {
            var scalarValue = propertyValue as ScalarValue;
            if (scalarValue != null)
                return scalarValue.Value?.ToString();

            return propertyValue.ToString();
        }

        private static string GeneratePartitionKey(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        private static string Truncate(string str)
        {
            if (str == null)
                return null;

            // See: https://blogs.msdn.microsoft.com/avkashchauhan/2011/11/30/how-the-size-of-an-entity-is-caclulated-in-windows-azure-table-storage/
            // String – # of Characters * 2 bytes + 4 bytes for length of string
            // Max coumn size is 64 Kb, so max string len is (65536 - 4) / 2 = 32766
            // 3 - is for "..."
            const int maxLength = 32766 - 3;

            if (str.Length > maxLength)
            {
                var builder = new StringBuilder();

                builder.Append(str, 0, maxLength);
                builder.Append("...");

                return builder.ToString();
            }

            return str;
        }

        private static string GetLogLevelString(LogEventLevel logLevel, IReadOnlyDictionary<string, LogEventPropertyValue> properties)
        {
            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                    return "trace";
                case LogEventLevel.Debug:
                    return "debug";
                case LogEventLevel.Information:
                    return "info";
                case LogEventLevel.Warning:
                    if (properties.ContainsKey("Monitor"))
                        return "monitor";
                    return "warning";
                case LogEventLevel.Error:
                    return "error";
                case LogEventLevel.Fatal:
                    return "critical";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}