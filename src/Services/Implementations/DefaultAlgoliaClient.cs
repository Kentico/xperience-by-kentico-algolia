using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Xperience.Algolia.Attributes;
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
        private readonly IAlgoliaObjectGenerator algoliaObjectGenerator;
        private readonly IEventLogService eventLogService;
        private readonly IProgressiveCache progressiveCache;
        private readonly ISearchClient searchClient;
        private const string CACHEKEY_STATISTICS = "Algolia|ListIndices";


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IAlgoliaObjectGenerator algoliaObjectGenerator,
            IEventLogService eventLogService,
            IProgressiveCache progressiveCache,
            ISearchClient searchClient)
        {
            this.algoliaIndexService = algoliaIndexService;
            this.algoliaObjectGenerator = algoliaObjectGenerator;
            this.eventLogService = eventLogService;
            this.progressiveCache = progressiveCache;
            this.searchClient = searchClient;
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


        public async Task<IEnumerable<IndicesResponse>> GetStatistics(CancellationToken cancellationToken)
        {
            return await progressiveCache.LoadAsync(async (cs) => {
                var response = await searchClient.ListIndicesAsync(ct: cancellationToken).ConfigureAwait(false);
                return response.Items;
            }, new CacheSettings(20, CACHEKEY_STATISTICS)).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public async Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> items, CancellationToken cancellationToken)
        {
            var successfulOperations = 0;

            // Group queue items based on index name
            var groups = items.GroupBy(item => item.IndexName);
            foreach (var group in groups)
            {
                try
                {
                    var algoliaIndex = IndexStore.Instance.Get(group.Key);
                    var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE);
                    var updateTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.UPDATE || queueItem.TaskType == AlgoliaTaskType.CREATE);
                    var upsertData = updateTasks.Select(queueItem => algoliaObjectGenerator.GetTreeNodeData(queueItem.Node, algoliaIndex.Type, queueItem.TaskType));
                    var deleteData = deleteTasks.Select(queueItem => queueItem.Node.DocumentID.ToString());

                    successfulOperations += await UpsertRecords(upsertData, group.Key, cancellationToken);
                    successfulOperations += await DeleteRecords(deleteData, group.Key, cancellationToken);
                }
                catch (Exception ex)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), ex.Message);
                }
            }

            return successfulOperations;
        }


        /// <inheritdoc />
        public Task Rebuild(string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = IndexStore.Instance.Get(indexName);
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
            CacheHelper.Remove(CACHEKEY_STATISTICS);
            
            var indexedNodes = new List<TreeNode>();
            var includedPathAttributes = algoliaIndex.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            foreach (var includedPathAttribute in includedPathAttributes)
            {
                var query = new MultiDocumentQuery()
                    .Path(includedPathAttribute.AliasPath)
                    .PublishedVersion()
                    .WithCoupledColumns();

                if (includedPathAttribute.PageTypes.Length > 0)
                {
                    query.Types(includedPathAttribute.PageTypes);
                }

                indexedNodes.AddRange(query.TypedResult);
            }

            var data = indexedNodes.Select(node => algoliaObjectGenerator.GetTreeNodeData(node, algoliaIndex.Type, AlgoliaTaskType.CREATE));
            var searchIndex = await algoliaIndexService.InitializeIndex(algoliaIndex.IndexName, cancellationToken);
            await searchIndex.ReplaceAllObjectsAsync(data, ct: cancellationToken).ConfigureAwait(false);
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
