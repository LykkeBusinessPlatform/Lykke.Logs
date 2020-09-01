using System;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.Logs.LykkeSanitizing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lykke.Logs
{
    /// <summary>
    /// Extension methods to register Lykke logging in the app services
    /// </summary>
    [PublicAPI]
    public static class LoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Lykke logging services to the specified <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        [NotNull]
        public static IServiceCollection AddLykkeLogging([NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (AppEnvironment.EnvInfo == null)
                throw new InvalidOperationException(
                    "ENV_INFO environment should be not empty. If you run application in your local machine, please fill up ENV_INFO with your name.");

            services.AddSingleton<IHealthNotifier, HealthNotifier>(s => new HealthNotifier(s.GetRequiredService<ILoggerFactory>()));

            services.AddSingleton<ILogFactory, LogFactory>(s => new LogFactory(
                s.GetRequiredService<ILoggerFactory>(),
                s.GetRequiredService<IHealthNotifier>(),
                s.GetService<IOptions<SanitizingOptions>>()));

            services.AddLogging();

            return services;
        }
    }
}