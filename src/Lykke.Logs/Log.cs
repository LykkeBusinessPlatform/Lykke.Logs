using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class Log : ILog
    {
        private static Func<Dictionary<string, object>, Exception, string> _messageFormatter =
            (parameters, exception) => exception == null
                ? parameters[nameof(LogEntryParameters.Message)]?.ToString()
                : (parameters[nameof(LogEntryParameters.Message)] == null
                    ? exception.ToString()
                    : $"{parameters[nameof(LogEntryParameters.Message)]} : {exception}");

        private readonly ILogger _logger;
        private readonly IHealthNotifier _healthNotifier;

        public Log(ILogger logger, IHealthNotifier healthNotifier)
        {
            _logger = logger;
            _healthNotifier = healthNotifier;
        }

        void ILog.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logParams = state as LogEntryParameters;
            if (logParams == null)
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
            else
            {
                var newState = new Dictionary<string, object>()
                {
                    { "{OriginalFormat}", "{Message:l}"},
                    { "Component", Path.GetFileName(logParams.CallerFilePath) },
                    { nameof(LogEntryParameters.Process), logParams.Process },
                    { nameof(LogEntryParameters.Context), logParams.Context },
                    { nameof(LogEntryParameters.Message), logParams.Message },
                    { nameof(LogEntryParameters.AppName), logParams.AppName },
                    { nameof(LogEntryParameters.AppVersion), logParams.AppVersion },
                    { nameof(LogEntryParameters.EnvInfo), logParams.EnvInfo },
                };
                _logger.Log(logLevel, eventId, newState, exception, _messageFormatter);
            }
        }

        bool ILog.IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        IDisposable ILog.BeginScope(string scopeMessage)
        {
            return _logger.BeginScope(scopeMessage);
        }

        #region Obsolete methods

        Task ILog.WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            this.Info(process, info, context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            _healthNotifier.Notify(info, context);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime)
        {
            this.Warning(process, info, context: context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime)
        {
            this.Warning(process, info, ex, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Error(process, exception, null, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime)
        {
            this.Critical(process, exception, null, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteInfoAsync(string process, string context, string info, DateTime? dateTime)
        {
            this.Info(process, info, context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteMonitorAsync(string process, string context, string info, DateTime? dateTime)
        {
            _healthNotifier.Notify(info, context);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, DateTime? dateTime)
        {
            this.Warning(process, info, context: context, moment: dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime)
        {
            this.Warning(process, info, ex, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Error(process, exception, null, context, dateTime);

            return Task.CompletedTask;
        }

        Task ILog.WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime)
        {
            this.Critical(process, exception, null, context, dateTime);

            return Task.CompletedTask;
        }

        #endregion
    }
}