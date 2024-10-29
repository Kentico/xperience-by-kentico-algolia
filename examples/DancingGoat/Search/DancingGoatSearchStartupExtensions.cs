using DancingGoat.Search.Services;
using Kentico.Xperience.Algolia;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddKenticoAlgoliaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKenticoAlgolia(builder => {
            builder.RegisterStrategy<AdvancedSearchIndexingStrategy>("DancingGoatAdvancedExampleStrategy");
            builder.RegisterStrategy<SimpleSearchIndexingStrategy>("DancingGoatMinimalExampleStrategy");
            builder.RegisterStrategy<ReusableContentItemsIndexingStrategy>(nameof(ReusableContentItemsIndexingStrategy));
        }, configuration);

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();

        services.AddSingleton<SimpleSearchService>();
        services.AddSingleton<AdvancedSearchService>();

        return services;
    }
}

