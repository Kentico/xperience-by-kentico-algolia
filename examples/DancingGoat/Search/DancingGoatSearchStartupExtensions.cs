using DancingGoat.Search.Services;
using Kentico.Xperience.Algolia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddAlgoliaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAlgolia(builder => {
            builder.RegisterStrategy<AdvancedSearchIndexingStrategy>("DancingGoatAdvancedExampleStrategy");
            builder.RegisterStrategy<SimpleSearchIndexingStrategy>("DancingGoatMinimalExampleStrategy");
        }, configuration);

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();

        services.AddSingleton<SimpleSearchService>();
        services.AddSingleton<AdvancedSearchService>();

        return services;
    }
}

