﻿using System;
using System.Threading;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Logs;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ConsoleLoggerRunner
{
    internal static class Program
    {
        private static void Main()
        {
            Environment.SetEnvironmentVariable("ENV_INFO", "test");

            using (var lf = LoggerFactory.Create(builder => builder.AddConsole()))
            using (var provider = new LogFactory(lf, new HealthNotifier(lf)))
            {
                var logger = provider.CreateLog("ComponentName");

                logger.Info("test");

                for (int i = 0; i < 100; i++)
                {
                    logger.Log(LogLevel.Warning, new EventId(0), GetState(), null,
                        (parameters, exception) => parameters.Message);
                    logger.Log(LogLevel.Critical, new EventId(0), GetState(), new OutOfMemoryException("Good buy"),
                        (parameters, exception) => parameters.Message);
                }

                logger.Log(LogLevel.Trace, new EventId(0), GetState(), null,
                    (parameters, exception) => parameters.Message);
                logger.Log(LogLevel.Debug, new EventId(0), GetState(), null,
                    (parameters, exception) => parameters.Message);
                logger.Log(LogLevel.Information, new EventId(0), GetState(), null,
                    (parameters, exception) => $"A long{Environment.NewLine}message");
                logger.Log(LogLevel.Warning, new EventId(0), GetState(), null,
                    (parameters, exception) => parameters.Message);


                logger.Log(LogLevel.Error, new EventId(0), GetState(),
                    new InvalidOperationException("Something goes wrong"),
                    (parameters, exception) => parameters.Message);
                logger.Log(LogLevel.Critical, new EventId(0), GetState(), new OutOfMemoryException("Good buy"),
                    (parameters, exception) => parameters.Message);


                Thread.Sleep(100);

                using (var scopedProvider = new LogFactory(lf, new HealthNotifier(lf)))
                {
                    var scopedLogger = scopedProvider.CreateLog("ScopedComponent");
                    scopedLogger.Log(LogLevel.Information, new EventId(0), GetState(), null,
                        (parameters, exception) => "Begin scope");

                    using (scopedLogger.BeginScope("Hi I am a scope {0}"))
                    {
                        scopedLogger.Log(LogLevel.Information, new EventId(0), GetState(), null,
                            (parameters, exception) => parameters.Message);
                        scopedLogger.Log(LogLevel.Error, new EventId(0), GetState(),
                            new InvalidOperationException("Something goes wrong"),
                            (parameters, exception) => parameters.Message);
                        scopedLogger.Log(LogLevel.Critical, new EventId(0), GetState(),
                            new OutOfMemoryException("Good buy"), (parameters, exception) => parameters.Message);
                    }

                    scopedLogger.Log(LogLevel.Information, new EventId(0), GetState(), null,
                        (parameters, exception) => "End scope");

                    Console.ReadLine();
                }
            }
        }

        private static LogEntryParameters GetState()
        {
            var state = new LogEntryParameters("AppName", "1.01", "Env", "Caller", "Process1", 12, "MyMessage", new { Prop1 = new { Prop11 = "11", Prop12 = "12" }, Prop2 = "OtherValue" }, DateTime.Now);
            return state;
        }
    }
}
