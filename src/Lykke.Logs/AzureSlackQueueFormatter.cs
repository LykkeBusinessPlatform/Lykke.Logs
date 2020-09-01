using System;
using System.IO;
using System.Text;
using AsyncFriendlyStackTrace;
using Common;
using Lykke.Common;
using Serilog.Events;
using Serilog.Formatting;

namespace Lykke.Logs
{
    internal class SlackQueueEntry
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
    }

    internal class AzureSlackQueueFormatter : ITextFormatter
    {
        private const string ComponentPropertyName = "Component";
        private const string ProcessPropertyName = "Process";
        private const string ContextPropertyName = "Context";
        private const string MessagePropertyName = "Message";
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";

        public void Format(LogEvent logEvent, TextWriter output)
        {
            string message = null;
            string component = null;
            string process = null;
            string context = null;

            if (logEvent.Properties.TryGetValue(ComponentPropertyName, out var componentValue))
                component = GetPropertyValue(componentValue);
            if (logEvent.Properties.TryGetValue(ProcessPropertyName, out var processValue))
                process = GetPropertyValue(processValue);
            if (logEvent.Properties.TryGetValue(ContextPropertyName, out var contextValue))
                context = GetPropertyValue(contextValue);
            if (logEvent.Properties.TryGetValue(MessagePropertyName, out var messageValue))
                message = GetPropertyValue(messageValue);
            if (logEvent.Exception == null && string.IsNullOrWhiteSpace(message))
                message = logEvent.RenderMessage();

            var entry = new SlackQueueEntry
            {
                Type = logEvent.Level.ToString(),
                Sender = GetSender(
                    logEvent.Level,
                    logEvent.Timestamp.UtcDateTime,
                    component,
                    process),
                Message = FormatMessage(
                    message,
                    context,
                    logEvent.Exception),
            };

            var text = entry.ToJson(ignoreNulls: true);

            output.Write(text);
        }

        private static string GetPropertyValue(LogEventPropertyValue propertyValue)
        {
            var scalarValue = propertyValue as ScalarValue;
            if (scalarValue != null)
                return scalarValue.Value?.ToString();

            return propertyValue.ToString();
        }

        private string FormatMessage(
            string message,
            string context,
            Exception exception)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(message))
                sb.Append(message);
            if (exception != null)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(exception.ToAsyncString());
            }
            if (!string.IsNullOrWhiteSpace(context))
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(context);
            }
            return sb.ToString();
        }

        private string GetSender(
            LogEventLevel logLevel,
            DateTime moment,
            string component,
            string process)
        {
            var sb = new StringBuilder($"[{moment.ToString(TimestampFormat)}] ");

            sb.Append($"{GetLogLevelString(logLevel)} {AppEnvironment.Name} {AppEnvironment.Version}");

            if (!string.IsNullOrWhiteSpace(AppEnvironment.EnvInfo))
                sb.Append($" : {AppEnvironment.EnvInfo}");

            sb.Append($" : {component}");

            if (!string.IsNullOrWhiteSpace(process))
                sb.Append($" : {process}");

            return sb.ToString();
        }

        private static string GetLogLevelString(LogEventLevel logLevel)
        {
            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                    return ":spiral_note_pad:";
                case LogEventLevel.Debug:
                    return ":computer:";
                case LogEventLevel.Information:
                    return ":information_source:";
                case LogEventLevel.Warning:
                    return ":warning:";
                case LogEventLevel.Error:
                    return ":exclamation:";
                case LogEventLevel.Fatal:
                    return ":no_entry:";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
