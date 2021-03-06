using System;
using System.Collections;
using System.Diagnostics;
using AsyncFriendlyStackTrace;

namespace Lykke.Logs.LykkeSanitizing
{
    internal sealed class SanitizingException : Exception
    {
        private readonly Exception _exception;
        private readonly Func<string, string> _sanitizer;

        private const string AsyncStackTraceExceptionData = "AsyncFriendlyStackTrace";

        public SanitizingException(Exception exception, Func<string, string> sanitizer)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
            _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));

            // add stack trace to be retrieved by AsyncFriendlyStackTrace later
            if (!Data.Contains(AsyncStackTraceExceptionData))
                Data.Add(AsyncStackTraceExceptionData, new StackTrace(exception, true).ToAsyncString());
        }

        public override string Message => _sanitizer(_exception.Message);
        public override string Source => _sanitizer(_exception.Source);
        public override string ToString() => _sanitizer(_exception.ToString());
        public override string StackTrace => _exception.StackTrace;
        public override IDictionary Data => _exception.Data;
    }
}