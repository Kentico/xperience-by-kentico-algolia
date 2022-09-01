using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Newtonsoft.Json.Linq;

[assembly: RegisterImplementation(typeof(IAlgoliaClient), typeof(DefaultAlgoliaClient), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaClient"/>.
    /// </summary>
    internal class DefaultAlgoliaClient : IAlgoliaClient
    {
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly IAlgoliaObjectGenerator algoliaObjectGenerator;
        private readonly IConversionService conversionService;
        private readonly IEventLogService eventLogService;
        private readonly IMediaFileInfoProvider mediaFileInfoProvider;
        private readonly IMediaFileUrlRetriever mediaFileUrlRetriever;
        private readonly IProgressiveCache progressiveCache;
        private readonly ISearchClient searchClient;
        private readonly string[] ignoredPropertiesForTrackingChanges = new string[] {
            nameof(AlgoliaSearchModel.ObjectID),
            nameof(AlgoliaSearchModel.Url),
            nameof(AlgoliaSearchModel.ClassName)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IConversionService conversionService,
            IEventLogService eventLogService,
            IMediaFileInfoProvider mediaFileInfoProvider,
            IMediaFileUrlRetriever mediaFileUrlRetriever,
            IProgressiveCache progressiveCache,
            ISearchClient searchClient)
        {
            this.algoliaIndexService = algoliaIndexService;
            this.conversionService = conversionService;
            this.algoliaObjectGenerator = algoliaObjectGenerator;
            this.eventLogService = eventLogService;
            this.progressiveCache = progressiveCache;
            this.searchClient = searchClient;
        }


        /// <inheritdoc />
        public Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (objectIds == null || !objectIds.Any())
            {
                return Task.FromResult(0);
            }

            return DeleteRecordsInternal(objectIds, indexName);
        }


        public async Task<List<IndicesResponse>> GetStatistics()
        {
            return await progressiveCache.LoadAsync(async (cs) => {
                var response = await searchClient.ListIndicesAsync().ConfigureAwait(false);
                return response.Items;
            }, new CacheSettings(20, "Algolia|ListIndices")).ConfigureAwait(false);
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
            cancellationToken.ThrowIfCancellationRequested();
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = IndexStore.Instance.Get(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"The index '{indexName}' is not registered.");
            }

            return RebuildInternal(algoliaIndex);
        }


        /// <inheritdoc />
        public Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (dataObjects == null || !dataObjects.Any())
            {
                return Task.FromResult(0);
            }

            return UpsertRecordsInternal(dataObjects, indexName);
        }


        private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string indexName)
        {
            var deletedCount = 0;
            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(objectIds).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }


        private async Task RebuildInternal(AlgoliaIndex algoliaIndex)
        {
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
            var searchIndex = algoliaIndexService.InitializeIndex(algoliaIndex.IndexName);
            await searchIndex.ReplaceAllObjectsAsync(data).ConfigureAwait(false);
        }


        private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName)
        {
            var upsertedCount = 0;
            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var batchIndexingResponse = await searchIndex.PartialUpdateObjectsAsync(dataObjects, createIfNotExists: true).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                upsertedCount += response.ObjectIDs.Count();
            }

            return upsertedCount;
        }
    }
}
