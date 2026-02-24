#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Configuration;

namespace Tahsilat.NET.Extensions
{
    /// <summary>
    /// Tahsilat.NET DI extensions method
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers ITahsilatClient as a singleton in the DI container.
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="configureOptions">Options configuration delegate</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddTahsilat(
            this IServiceCollection services,
            Action<TahsilatClientOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            // ITahsilatClient → Singleton register
            services.TryAddSingleton<ITahsilatClient>(sp =>
            {
                var options = new TahsilatClientOptions();
                configureOptions(options);
                return new TahsilatClient(options);
            });

            return services;
        }
    }
}
#endif
