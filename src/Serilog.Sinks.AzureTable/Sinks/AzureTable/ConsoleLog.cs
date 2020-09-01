using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Serilog.Sinks.AzureTable.Sinks.AzureTable
{
    internal class ConsoleLog : ILog
    {
        public IDisposable BeginScope(string scopeMessage)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
            where TState : LogEntryParameters
        {
            var message = formatter == null
                ? (state.Message ?? exception.ToString())
                : formatter(state, exception);
            Console.WriteLine($"[{logLevel}] {state.Moment:MM-dd HH:mm:ss.fff} : {message}");
        }

        public Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Error}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {exception}");
            return Task.CompletedTask;
        }

        public Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Error}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {exception}");
            return Task.CompletedTask;
        }

        public Task WriteFatalErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Critical}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {exception}");
            return Task.CompletedTask;
        }

        public Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Critical}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {exception}");
            return Task.CompletedTask;
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Information}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Information}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[Monitor] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[Monitor] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Warning}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Warning}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info} : {ex}");
            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Warning}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info}");
            return Task.CompletedTask;
        }

        public Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            Console.WriteLine($"[{LogLevel.Warning}] {(dateTime ?? DateTime.UtcNow):MM-dd HH:mm:ss.fff} : {context} : {info} : {ex}");
            return Task.CompletedTask;
        }
    }
}
