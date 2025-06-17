using Algolia.Search.Clients;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Default implementation of <see cref="IAlgoliaIndexService"/>.
/// </summary>
internal class DefaultAlgoliaIndexService : IAlgoliaIndexService
{
    private readonly ISearchClient searchClient;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAlgoliaIndexService"/> class.
    /// </summary>
    public DefaultAlgoliaIndexService(
        ISearchClient searchClient,
        IServiceProvider serviceProvider)
    {
        this.searchClient = searchClient;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<ISearchIndex> InitializeIndex(string indexName, CancellationToken cancellationToken)
    {
        var algoliaIndex = AlgoliaIndexStore.Instance.GetIndex(indexName) ?? throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");

        var algoliaStrategy = serviceProvider.GetRequiredStrategy(algoliaIndex);
        var indexSettings = algoliaStrategy.GetAlgoliaIndexSettings();

        indexSettings.AttributesToRetrieve ??= [];

        indexSettings.AttributesToRetrieve.Add(BaseJObjectProperties.OBJECT_ID);
        indexSettings.AttributesToRetrieve.Add(BaseJObjectProperties.URL);
        indexSettings.AttributesToRetrieve.Add(BaseJObjectProperties.CONTENT_TYPE_NAME);
        indexSettings.AttributesToRetrieve.Add(BaseJObjectProperties.LANGUAGE_NAME);

        var searchIndex = searchClient.InitIndex(indexName);
        await searchIndex.SetSettingsAsync(indexSettings, ct: cancellationToken);

        return searchIndex;
    }
}
