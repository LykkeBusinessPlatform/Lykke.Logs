using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Logs.LykkeSanitizing
{
    public static class SanitizingLogBuilderExtensions
    {
        /// <summary>
        /// Adds sensitive pattern that should not be logged. Api keys, private keys and so on.
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="pattern">Regex to recognize data that should be replaced.</param>
        /// <param name="replacement">String to insert, can be empty string.</param>
        /// <returns><see cref="IServiceCollection"/> instance to continue configuring.</returns>
        public static IServiceCollection AddSanitizingFilter(this IServiceCollection services, Regex pattern, string replacement)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.Configure<SanitizingOptions>(options => options.Filters.Add(new SanitizingFilter(pattern, replacement)));

            return services;
        }
    }
}