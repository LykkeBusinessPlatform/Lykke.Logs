﻿using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SlackNotifications;

namespace Lykke.Logs.Slack
{
    /// <summary>
    /// Logs entries to the specified Slack channel. Which types of entries should be logged, can be configured
    /// </summary>
    [PublicAPI]
    public sealed class LykkeLogToSlack : ILog
    {
        private readonly ISlackNotificationsSender _sender;
        private readonly string _channel;
        private readonly bool _isInfoEnabled;
        private readonly bool _isMonitorEnabled;
        private readonly bool _isWarningEnabled;
        private readonly bool _isErrorEnabled;
        private readonly bool _isFatalErrorEnabled;
        private readonly string _componentNamePrefix;
        private readonly TimeSpan _sameMessageMutePeriod = TimeSpan.FromSeconds(60);
        private readonly ConcurrentDictionary<LogLevel, DateTime> _lastTimes = new ConcurrentDictionary<LogLevel, DateTime>();
        private readonly ConcurrentDictionary<LogLevel, string> _lastMessages = new ConcurrentDictionary<LogLevel, string>();

        private LykkeLogToSlack(ISlackNotificationsSender sender, string channel, LogLevel logLevel)
        {
            _sender = sender;
            _channel = channel;

            _isInfoEnabled = logLevel.HasFlag(LogLevel.Info);
            _isMonitorEnabled = logLevel.HasFlag(LogLevel.Monitoring);
            _isWarningEnabled = logLevel.HasFlag(LogLevel.Warning);
            _isErrorEnabled = logLevel.HasFlag(LogLevel.Error);
            _isFatalErrorEnabled = logLevel.HasFlag(LogLevel.FatalError);

            _componentNamePrefix = GetComponentNamePrefix();
        }

        /// <summary>
        /// Creates logger with, which logs entries of the given <paramref name="logLevel"/>,
        /// to the given <paramref name="channel"/>, using given <paramref name="sender"/>
        /// </summary>
        public static ILog Create(ISlackNotificationsSender sender, string channel, LogLevel logLevel = LogLevel.All)
        {
            return new LykkeLogToSlack(sender, channel, logLevel);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isInfoEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                if (IsSameMessage(LogLevel.Info, message))
                    return Task.CompletedTask;

                return _sender.SendAsync(_channel, ":information_source:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isMonitorEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                if (IsSameMessage(LogLevel.Monitoring, message))
                    return Task.CompletedTask;

                return _sender.SendAsync(_channel, ":loudspeaker:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            if (_isWarningEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {info} : {context}";
                if (IsSameMessage(LogLevel.Warning, message))
                    return Task.CompletedTask;

                return _sender.SendAsync(_channel, ":warning:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            if (_isWarningEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {ex} : {info} : {context}";
                if (IsSameMessage(LogLevel.Warning, message))
                    return Task.CompletedTask;

                return _sender.SendAsync(_channel, ":warning:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            if (_isErrorEnabled)
            {
                var message = $"{GetComponentName(component)} : {process} : {exception} : {context}";
                if (IsSameMessage(LogLevel.Error, message))
                    return Task.CompletedTask;

                return _sender.SendAsync(_channel, ":exclamation:", message);
            }

            return Task.CompletedTask;
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            if (_isFatalErrorEnabled)
            {
                return _sender.SendAsync(_channel, ":no_entry:", $"{GetComponentName(component)} : {process} : {exception} : {context}");
            }

            return Task.CompletedTask;
        }

        public Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteInfoAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteMonitorAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            return WriteWarningAsync(AppEnvironment.Name, process, context, info, dateTime);
        }

        public Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            return WriteWarningAsync(AppEnvironment.Name, process, context, info, ex, dateTime);
        }

        public Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return WriteErrorAsync(AppEnvironment.Name, process, context, exception, dateTime);
        }

        public Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            return WriteFatalErrorAsync(AppEnvironment.Name, process, context, exception, dateTime);
        }

        private string GetComponentNamePrefix()
        {
            var sb = new StringBuilder();

            sb.Append($"{AppEnvironment.Name} {AppEnvironment.Version}");

            if (!string.IsNullOrWhiteSpace(AppEnvironment.EnvInfo))
            {
                sb.Append($" : {AppEnvironment.EnvInfo}");
            }

            return sb.ToString();
        }

        private string GetComponentName(string component)
        {
            if (AppEnvironment.Name == null || !AppEnvironment.Name.StartsWith(component))
                return string.Concat(_componentNamePrefix, " : ", component);
            return _componentNamePrefix;
        }

        private bool IsSameMessage(LogLevel level, string message)
        {
            var now = DateTime.UtcNow;
            if (_lastTimes.TryGetValue(level, out DateTime lastTime))
            {
                if (_lastMessages.TryGetValue(level, out string lastMessage))
                {
                    if (lastMessage == message && now - lastTime < _sameMessageMutePeriod)
                        return true;
                    _lastMessages.TryUpdate(level, message, lastMessage);
                }
                _lastTimes.TryUpdate(level, now, lastTime);
            }
            else
            {
                _lastTimes.TryAdd(level, now);
                _lastMessages.TryAdd(level, message);
            }
            return false;
        }
    }
}