﻿using System;
using System.Collections.Generic;
using Lykke.Common;
using Lykke.Common.Log;
using Microsoft.Extensions.Logging;

namespace Lykke.Logs
{
    internal sealed class HealthNotifier : IHealthNotifier
    {
        private static Func<Dictionary<string, object>, Exception, string> _messageFormatter =
            (parameters, exception) => parameters[nameof(LogEntryParameters.Message)]?.ToString();

        private readonly ILogger<HealthNotifier> _logger;

        public HealthNotifier(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HealthNotifier>();
        }

        public void Dispose()
        {
        }

        public void Notify(string healthMessage, object context = null)
        {
            var state = new Dictionary<string, object>()
                {
                    { "Monitor", true },
                    { "{OriginalFormat}", "{Message:l}"},
                    { nameof(LogEntryParameters.Context), context },
                    { nameof(LogEntryParameters.Message), healthMessage },
                    { nameof(LogEntryParameters.AppName), AppEnvironment.Name },
                    { nameof(LogEntryParameters.AppVersion), AppEnvironment.Version },
                    { nameof(LogEntryParameters.EnvInfo), AppEnvironment.EnvInfo },
                };
            _logger.Log(LogLevel.Warning, 0, state, null, _messageFormatter);
        }
    }
}