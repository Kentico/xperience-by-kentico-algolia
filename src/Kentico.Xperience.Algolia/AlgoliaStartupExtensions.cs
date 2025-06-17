using System.Diagnostics;

using Algolia.Search.Clients;

using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Algolia;

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
    private static IServiceCollection AddKenticoAlgoliaInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var algoliaSection = configuration.GetSection(AlgoliaOptions.CMS_ALGOLIA_SECTION_NAME);
        var algoliaOptions = algoliaSection.GetChildren();

        bool isConfigured = false;

        if (algoliaOptions.Single(x => x.Key == nameof(AlgoliaOptions.SearchKey)).Value != string.Empty
            && algoliaOptions.Single(x => x.Key == nameof(AlgoliaOptions.ApiKey)).Value != string.Empty
            && algoliaOptions.Single(x => x.Key == nameof(AlgoliaOptions.ApplicationId)).Value != string.Empty)
        {
            isConfigured = true;
        }

        services
            .Configure<AlgoliaOptions>(algoliaSection)
            .PostConfigure<AlgoliaOptions>(options => options.IsConfigured = isConfigured);

        if (!isConfigured)
        {
            return services;
        }

        return services
            .AddSingleton<AlgoliaModuleInstaller>()
            .AddSingleton<ISearchClient>(s =>
            {
                var options = s.GetRequiredService<IOptions<AlgoliaOptions>>();
                var configuration = new SearchConfig(options.Value.ApplicationId, options.Value.ApiKey);
                var fileVersion = FileVersionInfo.GetVersionInfo(typeof(AlgoliaOptions).Assembly.Location);
                string versioNumber = new Version(fileVersion.FileVersion ?? string.Empty).ToString(3);
                configuration.DefaultHeaders["User-Agent"] = $"Kentico Xperience for Algolia ({versioNumber})";

                return new SearchClient(configuration);
            })
           .AddSingleton<IAlgoliaClient, DefaultAlgoliaClient>()
           .AddSingleton<IAlgoliaTaskLogger, DefaultAlgoliaTaskLogger>()
           .AddSingleton<IAlgoliaTaskProcessor, DefaultAlgoliaTaskProcessor>()
           .AddSingleton<IAlgoliaConfigurationStorageService, DefaultAlgoliaConfigurationStorageService>()
           .AddSingleton<IAlgoliaIndexService, DefaultAlgoliaIndexService>();
    }
    /// <summary>
    /// Adds Algolia services and custom module to application with customized options provided by the <see cref="IAlgoliaBuilder"/>
    /// in the <paramref name="configure" /> action.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configure"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAlgolia(this IServiceCollection serviceCollection, Action<IAlgoliaBuilder> configure, IConfiguration configuration)
    {
        serviceCollection.AddKenticoAlgoliaInternal(configuration);

        var builder = new AlgoliaBuilder(serviceCollection);

        configure(builder);

        if (builder.IncludeDefaultStrategy)
        {
            serviceCollection.AddTransient<DefaultAlgoliaIndexingStrategy>();
            builder.RegisterStrategy<DefaultAlgoliaIndexingStrategy>("Default");
        }

        return serviceCollection;
    }

    /// <summary>
    /// Adds Algolia services and custom module to application with <see cref="DefaultAlgoliaIndexingStrategy"/>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAlgolia(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddKenticoAlgoliaInternal(configuration);

        var builder = new AlgoliaBuilder(serviceCollection);

        serviceCollection.AddTransient<DefaultAlgoliaIndexingStrategy>();
        builder.RegisterStrategy<DefaultAlgoliaIndexingStrategy>("Default");

        return serviceCollection;
    }
}


public interface IAlgoliaBuilder
{
    /// <summary>
    /// Registers the given <typeparamref name="TStrategy" /> as a transient service under <paramref name="strategyName" />
    /// </summary>
    /// <typeparam name="TStrategy">The custom type of <see cref="IAlgoliaIndexingStrategy"/> </typeparam>
    /// <param name="strategyName">Used internally <typeparamref name="TStrategy" /> to enable dynamic assignment of strategies to search indexes. Names must be unique.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if an strategy has already been registered with the given <paramref name="strategyName"/>
    /// </exception>
    /// <returns></returns>
    IAlgoliaBuilder RegisterStrategy<TStrategy>(string strategyName) where TStrategy : class, IAlgoliaIndexingStrategy;
}

internal class AlgoliaBuilder : IAlgoliaBuilder
{
    private readonly IServiceCollection serviceCollection;

    /// <summary>
    /// If true, the <see cref="DefaultAlgoliaIndexingStrategy" /> will be available as an explicitly selectable indexing strategy
    /// within the Admin UI. Defaults to <c>true</c>
    /// </summary>
    public bool IncludeDefaultStrategy { get; set; } = true;

    public AlgoliaBuilder(IServiceCollection serviceCollection) => this.serviceCollection = serviceCollection;

    public IAlgoliaBuilder RegisterStrategy<TStrategy>(string strategyName) where TStrategy : class, IAlgoliaIndexingStrategy
    {
        StrategyStorage.AddStrategy<TStrategy>(strategyName);
        serviceCollection.AddTransient<TStrategy>();

        return this;
    }
}
