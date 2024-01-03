using System;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexService"/>.
    /// </summary>
    internal class DefaultAlgoliaIndexService : IAlgoliaIndexService
    {
        private readonly ISearchClient searchClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaIndexService"/> class.
        /// </summary>
        public DefaultAlgoliaIndexService(ISearchClient searchClient)
        {
            this.searchClient = searchClient;
        }

        /// <inheritdoc />
        public async Task<ISearchIndex> InitializeIndex(string indexName, CancellationToken cancellationToken)
        {
            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");
            }

            var indexSettings = algoliaIndex.IndexSettings;
            var searchIndex = searchClient.InitIndex(indexName);
            await searchIndex.SetSettingsAsync(indexSettings, ct: cancellationToken);

            return searchIndex;
        }
    }
}