using System;
using System.Linq;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Logs.LykkeSanitizing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    /// <summary>
    /// Log factory
    /// </summary>
    [PublicAPI]
    public sealed class LogFactory : ILogFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly SanitizingOptions _sanitizingOptions;
        private readonly IHealthNotifier _healthNotifier;

        internal LogFactory(
            ILoggerFactory loggerFactory,
            IHealthNotifier healthNotifier,
            IOptions<SanitizingOptions> sanitizingOptions = null)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _sanitizingOptions = sanitizingOptions?.Value ?? new SanitizingOptions();
            _healthNotifier = healthNotifier;
        }

        /// <summary>
        /// Creates empty log factory
        /// </summary>
        public static ILogFactory Create()
        {
            var lf = new LoggerFactory();
            return new LogFactory(lf, new HealthNotifier(lf));
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component, string componentNameSuffix)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrWhiteSpace(componentNameSuffix))
                throw new ArgumentException("Should be not empty string", nameof(componentNameSuffix));

            ILog log = new Log(
                _loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component, componentNameSuffix)),
                _healthNotifier);

            return _sanitizingOptions.Filters.Any()
                ? new SanitizingLog(log, _sanitizingOptions)
                : log;
        }

        /// <inheritdoc />
        public ILog CreateLog<TComponent>(TComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            ILog log = new Log(
                _loggerFactory.CreateLogger(ComponentNameHelper.GetComponentName(component)),
                _healthNotifier);

            return _sanitizingOptions.Filters.Any()
                ? new SanitizingLog(log, _sanitizingOptions)
                : log;
        }

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _loggerFactory.AddProvider(provider);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _loggerFactory.Dispose();
        }
    }
}