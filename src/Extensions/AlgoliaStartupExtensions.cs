using Algolia.Search.Clients;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;
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
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        public static IServiceCollection AddAlgolia(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<AlgoliaOptions>(configuration.GetSection(AlgoliaOptions.SECTION_NAME))
                .AddSingleton<IInsightsClient>(s =>
                {
                    var options = s.GetRequiredService<IOptions<AlgoliaOptions>>();

                    return new InsightsClient(options.Value.ApplicationId, options.Value.ApiKey);
                })
                .AddSingleton<AlgoliaModuleInstaller>()
                .AddSingleton<ISearchClient>(s =>
                {
                    var options = s.GetRequiredService<IOptions<AlgoliaOptions>>();
                    var configuration = new SearchConfig(options.Value.ApplicationId, options.Value.ApiKey);
                    configuration.DefaultHeaders["User-Agent"] = "Kentico Xperience for Algolia (4.0.0)";

                    return new SearchClient(configuration);
                });
        }

        public static IServiceCollection RegisterStrategy<TStrategy>(this IServiceCollection serviceCollection, string strategyName) where TStrategy : IAlgoliaIndexingStrategy, new()
        {
            StrategyStorage.AddStrategy<TStrategy>(strategyName);
            return serviceCollection;
        }
    }
}