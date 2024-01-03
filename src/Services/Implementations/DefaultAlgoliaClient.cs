using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;
using CMS.Websites;
using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaClient"/>.
    /// </summary>
    internal class DefaultAlgoliaClient : IAlgoliaClient
    {
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly ICacheAccessor cacheAccessor;
        private readonly IContentQueryExecutor executor;
        private readonly IProgressiveCache progressiveCache;
        private readonly ISearchClient searchClient;

        internal const string CACHEKEY_STATISTICS = "Algolia|ListIndices";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(
            IAlgoliaIndexService algoliaIndexService,
            ICacheAccessor cacheAccessor,
            IProgressiveCache progressiveCache,
            ISearchClient searchClient,
            IContentQueryExecutor executor)
        {
            this.algoliaIndexService = algoliaIndexService;
            this.cacheAccessor = cacheAccessor;
            this.progressiveCache = progressiveCache;
            this.searchClient = searchClient;
            this.executor = executor;
        }

        /// <inheritdoc />
        public Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (objectIds == null || !objectIds.Any())
            {
                return Task.FromResult(0);
            }

            return DeleteRecordsInternal(objectIds, indexName, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ICollection<IndicesResponse>> GetStatistics(CancellationToken cancellationToken) => 
            (await searchClient.ListIndicesAsync(ct: cancellationToken).ConfigureAwait(false)).Items;

        /// <inheritdoc />
        public Task Rebuild(string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"The index '{indexName}' is not registered.");
            }

            return RebuildInternal(algoliaIndex, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (dataObjects == null || !dataObjects.Any())
            {
                return Task.FromResult(0);
            }

            return UpsertRecordsInternal(dataObjects, indexName, cancellationToken);
        }

        private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
        {
            var deletedCount = 0;
            var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
            var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(objectIds, ct: cancellationToken).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }

        private async Task RebuildInternal(AlgoliaIndex algoliaIndex, CancellationToken cancellationToken)
        {
            // Clear statistics cache so listing displays updated data after rebuild
            cacheAccessor.Remove(CACHEKEY_STATISTICS);

            var indexedItems = new List<IndexedItemModel>();
            foreach (var includedPathAttribute in algoliaIndex.IncludedPaths)
            {
                foreach (var language in algoliaIndex.LanguageCodes)
                {
                    var queryBuilder = new ContentItemQueryBuilder();

                    if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Length > 0)
                    {
                        foreach (var contentType in includedPathAttribute.ContentTypes)
                        {
                            queryBuilder.ForContentType(contentType, config => config.WithLinkedItems(1).ForWebsite(algoliaIndex.WebSiteChannelName, includeUrlPath: true));
                        }
                    }
                    queryBuilder.InLanguage(language);

                    var webPageItems = (await executor.GetWebPageResult(queryBuilder, container => container, cancellationToken: cancellationToken))
                        .Select(x => new IndexedItemModel()
                        {
                            LanguageCode = language,
                            ClassName = x.ContentTypeName,
                            ChannelName = algoliaIndex.WebSiteChannelName,
                            WebPageItemGuid = x.WebPageItemGUID,
                            WebPageItemTreePath = x.WebPageItemTreePath
                        });

                    foreach (var item in webPageItems)
                    {
                        indexedItems.Add(item);
                    }
                }
            }

            var searchIndex = await algoliaIndexService.InitializeIndex(algoliaIndex.IndexName, cancellationToken);
            await searchIndex.ClearObjectsAsync(ct: cancellationToken);

            indexedItems.ForEach(node => AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(new AlgoliaQueueItem(node, AlgoliaTaskType.PUBLISH_INDEX, algoliaIndex.IndexName)));
        }

        private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
        {
            var upsertedCount = 0;
            var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
            var batchIndexingResponse = await searchIndex.PartialUpdateObjectsAsync(dataObjects, createIfNotExists: true, ct: cancellationToken).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                upsertedCount += response.ObjectIDs.Count();
            }

            return upsertedCount;
        }
    }
}
