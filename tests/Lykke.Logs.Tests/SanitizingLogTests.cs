using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Common.Log;

using Lykke.Common.Log;
using Lykke.Logs.LykkeSanitizing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NSubstitute;

using Xunit;

using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Lykke.Logs.Tests
{
    public class SanitizingLogTests
    {
        private readonly ISanitizingLog _log;
        private readonly ILogger _internalLog;

        public SanitizingLogTests()
        {
            _internalLog = Substitute.For<ILogger>();
            _log = new Log(_internalLog, Substitute.For<IHealthNotifier>()).Sanitize();
        }

        [Fact]
        public async Task ShouldSanitizeLog()
        {
            // Arrange

            _log.AddSanitizingFilter(new Regex(@"""privateKey"": ""(.*)"""), "\"privateKey\": \"*\"");

            var secret = "qwertyuiop";
            var patternedString = $"\"privateKey\": \"{secret}\"";
            var patternedObject = new { privateKey = secret };
            var patternedException = new Exception(patternedString);

            // Act
            // Get and call all available logging methods

            var extMethods = typeof(MicrosoftLoggingBasedLogExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
            var logMethods = typeof(ILog).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.StartsWith("Write"));

            foreach (var m in extMethods.Concat(logMethods))
            {
                var args = m.GetParameters()
                    .Select(p =>
                        p.ParameterType == typeof(string) ? patternedString :
                        p.ParameterType == typeof(object) ? patternedObject :
                        p.ParameterType == typeof(Exception) ? (object)patternedException :
                        p.ParameterType == typeof(ILog) ? (object)_log :
                        p.ParameterType == typeof(Level) ? (object)Level.Information :
                        p.ParameterType == typeof(int) ? (object)1 :
                        null)
                    .ToArray();

                if (m.Invoke(m.IsStatic ? null : _log, args) is Task task)
                    await task;
            }

            // Assert

            var writeMethodCalls = _internalLog.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name.StartsWith("Log"))
                .ToList();

            Assert.NotEmpty(writeMethodCalls);
            Assert.DoesNotContain(writeMethodCalls,
                c => c.GetArguments()
                    // We are looking for generic method argument LogEntryParameters
                    // It is introduced as a dictionary with string keys and object values
                    .OfType<Dictionary<string, object>>()
                    .Any(a =>
                    {
                        var messageContains = a["Message"] is string message && message.Contains(secret);
                        var contextContains = a["Context"] is string context && context.Contains(secret);
                        return messageContains || contextContains;
                    }));
        }

        [Fact]
        public void ShouldSanitizeAllPatterns()
        {
            // Arrange
            _log
                .AddSanitizingFilter(new Regex(@"""privateKey"": ""(.*)"""), "\"privateKey\": \"*\"")
                .AddSanitizingFilter(new Regex(@"""password"": ""(.*)"""), "\"password\": \"*\"");

            var secret = "qwertyuiop";
            var patternedString = $"\"privateKey\": \"{secret}\", \"password\": \"{secret}\"";
            var patternedObject = new { privateKey = secret, password = secret };

            // Act
            _log.Info(patternedString, patternedObject);

            // Assert
            var logMethodCalls = _internalLog.ReceivedCalls()
                .Where(c => c.GetMethodInfo().Name.StartsWith("Log"))
                .ToList();

            Assert.NotEmpty(logMethodCalls);
            Assert.DoesNotContain(logMethodCalls,
                c => c.GetArguments()
                    // We are looking for generic method argument LogEntryParameters
                    // It is introduced as a dictionary with string keys and object values
                    .OfType<Dictionary<string, object>>()
                    .Any(a => ((string)a["Message"]).Contains(secret) || ((string)a["Context"]).Contains(secret)));
        }

        [Fact]
        public void ShouldConfigureOptions()
        {
            // Arrange

            var serviceCollection = new ServiceCollection();

            // Act

            serviceCollection.Configure<SanitizingOptions>(x => x.Filters.Add(new SanitizingFilter(new Regex(""), "*")));
            serviceCollection.Configure<SanitizingOptions>(x => x.Filters.Add(new SanitizingFilter(new Regex(""), "#")));

            // Assert

            var options = serviceCollection.BuildServiceProvider().GetService<IOptions<SanitizingOptions>>();

            Assert.NotNull(options.Value);
            Assert.Equal(2, options.Value.Filters.Count);
            Assert.Contains(options.Value.Filters, f => f.Replacement == "*");
            Assert.Contains(options.Value.Filters, f => f.Replacement == "#");
        }

        [Fact]
        public void Sanitize_ValueIsNullWithoutFilters_NotThrowException()
        {
            // Arrange
            var fakeLog = Substitute.For<ILog>();
            var sanitizer = new SanitizingLog(fakeLog, new SanitizingOptions());

            // Act
            var result = sanitizer.Sanitize(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Sanitize_ValueIsNullWithFilters_NotThrowException()
        {
            // Arrange
            var fakeLog = Substitute.For<ILog>();
            var sanitizer = new SanitizingLog(fakeLog, new SanitizingOptions());
            sanitizer.AddSanitizingFilter(new Regex(""), "");

            // Act
            var result = sanitizer.Sanitize(null);

            // Assert
            Assert.Null(result);
        }
    }
}