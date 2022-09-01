using System;

using Algolia.Search.Clients;

using Kentico.Xperience.Algolia.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Algolia.Extensions
{
    /// <summary>
    /// Application startup extension methods.
    /// </summary>
    public static class AlgoliaStartupExtensions
    {
        /// <summary>
        /// Registers instances of <see cref="IInsightsClient"/> and <see cref="ISearchClient"/> with
        /// Dependency Injection and configures the Algolia options. Registers the provided <paramref name="indexes"/>
        /// with the <see cref="IndexStore"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="indexes">The Algolia indexes to register.</param>
        public static IServiceCollection AddAlgolia(this IServiceCollection services, IConfiguration configuration, params AlgoliaIndex[] indexes)
        {
            Array.ForEach(indexes, index => IndexStore.Instance.Add(index));

            return services
                .Configure<AlgoliaOptions>(configuration.GetSection(AlgoliaOptions.SECTION_NAME))
                .AddSingleton<IInsightsClient>(s =>
                {
                    var options = s.GetRequiredService<IOptions<AlgoliaOptions>>();

                    return new InsightsClient(options.Value.ApplicationId, options.Value.ApiKey);
                })
                .AddSingleton<ISearchClient>(s =>
                {
                    var options = s.GetRequiredService<IOptions<AlgoliaOptions>>();

                    return new SearchClient(options.Value.ApplicationId, options.Value.ApiKey);
                });
        }
    }
}