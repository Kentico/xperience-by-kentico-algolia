using System;

using Algolia.Search.Clients;

using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.AlgoliaSearch
{
    /// <summary>
    /// Application startup extension methods.
    /// </summary>
    public static class AlgoliaStartupExtensions
    {
        /// <summary>
        /// Registers instances of <see cref="IInsightsClient"/>, <see cref="ISearchClient"/>, and
        /// <see cref="IAlgoliaIndexStore"/> with Dependency Injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="store">The implementation of <see cref="IAlgoliaIndexStore"/> to register.</param>
        public static IServiceCollection AddAlgolia(this IServiceCollection services, IConfiguration configuration, IAlgoliaIndexStore store)
        {
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
                })
                .AddSingleton(store);
        }
    }
}