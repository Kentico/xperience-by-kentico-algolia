using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;
using CMS.WorkflowEngine;

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
        private readonly ICacheAccessor cacheAccessor;
        private readonly IEventLogService eventLogService;
        private readonly IVersionHistoryInfoProvider versionHistoryInfoProvider;
        private readonly IWorkflowStepInfoProvider workflowStepInfoProvider;
        private readonly IProgressiveCache progressiveCache;
        private readonly ISearchClient searchClient;
        private const string CACHEKEY_STATISTICS = "Algolia|ListIndices";


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IAlgoliaObjectGenerator algoliaObjectGenerator,
            ICacheAccessor cacheAccessor,
            IEventLogService eventLogService,
            IVersionHistoryInfoProvider versionHistoryInfoProvider,
            IWorkflowStepInfoProvider workflowStepInfoProvider,
            IProgressiveCache progressiveCache,
            ISearchClient searchClient)
        {
            this.algoliaIndexService = algoliaIndexService;
            this.algoliaObjectGenerator = algoliaObjectGenerator;
            this.cacheAccessor = cacheAccessor;
            this.eventLogService = eventLogService;
            this.versionHistoryInfoProvider = versionHistoryInfoProvider;
            this.workflowStepInfoProvider = workflowStepInfoProvider;
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


        /// <inheritdoc/>
        public async Task<ICollection<IndicesResponse>> GetStatistics(CancellationToken cancellationToken)
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
                    var deleteIds = new List<string>();
                    var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE);
                    deleteIds.AddRange(GetIdsToDelete(group.Key, deleteTasks));

                    var updateTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.UPDATE || queueItem.TaskType == AlgoliaTaskType.CREATE);
                    var upsertData = new List<JObject>();
                    foreach (var queueItem in updateTasks)
                    {
                        // There may be less fragments than previously indexed. Delete fragments created by the
                        // previous version of the node
                        deleteIds.AddRange(GetFragmentsToDelete(queueItem));
                        var data = GetDataToUpsert(queueItem);
                        upsertData.AddRange(data);
                    }

                    successfulOperations += await DeleteRecords(deleteIds, group.Key, cancellationToken);
                    successfulOperations += await UpsertRecords(upsertData, group.Key, cancellationToken);
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


        /// <summary>
        /// Gets the IDs of the fragments previously generated by a node update. Because the data that was split could
        /// be smaller than previous updates, if they were not deleted during an update task, there would be orphaned
        /// data in Algolia. When the <see cref="AlgoliaQueueItem.TaskType"/> is <see cref="AlgoliaTaskType.UPDATE"/>,
        /// we must check for a previous version and delete the fragments generated by that version, before upserting new fragments.
        /// </summary>
        /// <param name="queueItem">The item being processed.</param>
        /// <returns>A list of Algolia IDs that should be deleted, or an empty list.</returns>
        /// <exception cref="ArgumentNullException" />
        private IEnumerable<string> GetFragmentsToDelete(AlgoliaQueueItem queueItem)
        {
            var algoliaIndex = IndexStore.Instance.Get(queueItem.IndexName);
            if (queueItem.TaskType != AlgoliaTaskType.UPDATE || algoliaIndex.DistinctOptions == null)
            {
                // Only split data on UPDATE tasks if splitting is enabled
                return Enumerable.Empty<string>();
            }

            var publishedStepId = workflowStepInfoProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(WorkflowStepInfo.StepWorkflowID), queueItem.Node.WorkflowStep.StepWorkflowID)
                .WhereEquals(nameof(WorkflowStepInfo.StepType), WorkflowStepTypeEnum.DocumentPublished)
                .AsIDQuery()
                .GetScalarResult<int>(0);
            var previouslyPublishedVersionID = versionHistoryInfoProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(VersionHistoryInfo.DocumentID), queueItem.Node.DocumentID)
                .WhereEquals(nameof(VersionHistoryInfo.NodeSiteID), queueItem.Node.NodeSiteID)
                .WhereEquals(nameof(VersionHistoryInfo.VersionWorkflowStepID), publishedStepId)
                .OrderByDescending(nameof(VersionHistoryInfo.WasPublishedTo))
                .AsIDQuery()
                .GetScalarResult<int>(0);
            if (previouslyPublishedVersionID == 0)
            {
                return Enumerable.Empty<string>();
            }

            var previouslyPublishedNode = queueItem.Node.VersionManager.GetVersion(previouslyPublishedVersionID, queueItem.Node);
            var previouslyPublishedNodeData = algoliaObjectGenerator.GetTreeNodeData(new AlgoliaQueueItem(previouslyPublishedNode, AlgoliaTaskType.CREATE, algoliaIndex.IndexName));

            return algoliaObjectGenerator.SplitData(previouslyPublishedNodeData, algoliaIndex).Select(obj => obj.Value<string>("objectID"));
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


        private IEnumerable<JObject> GetDataToUpsert(AlgoliaQueueItem queueItem)
        {
            var algoliaIndex = IndexStore.Instance.Get(queueItem.IndexName);
            if (algoliaIndex.DistinctOptions != null)
            {
                // If the data is split, force CREATE type to push all data to Algolia
                var nodeData = algoliaObjectGenerator.GetTreeNodeData(new AlgoliaQueueItem(queueItem.Node, AlgoliaTaskType.CREATE, queueItem.IndexName));
                return algoliaObjectGenerator.SplitData(nodeData, algoliaIndex);
            }

            return new JObject[] { algoliaObjectGenerator.GetTreeNodeData(queueItem) };
        }


        private IEnumerable<string> GetIdsToDelete(string indexName, IEnumerable<AlgoliaQueueItem> deleteTasks)
        {
            var algoliaIndex = IndexStore.Instance.Get(indexName);
            if (algoliaIndex.DistinctOptions != null)
            {
                // Data has been split, get IDs of the smaller records
                var ids = new List<string>();
                foreach (var queueItem in deleteTasks)
                {
                    var data = GetDataToUpsert(queueItem);
                    ids.AddRange(data.Select(obj => obj.Value<string>("objectID")));
                }

                return ids;
            }

            return deleteTasks.Select(queueItem => queueItem.Node.DocumentID.ToString());
        }


        private async Task RebuildInternal(AlgoliaIndex algoliaIndex, CancellationToken cancellationToken)
        {
            // Clear statistics cache so listing displays updated data after rebuild
            cacheAccessor.Remove(CACHEKEY_STATISTICS);
            
            var indexedNodes = new List<TreeNode>();
            foreach (var includedPathAttribute in algoliaIndex.IncludedPaths)
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

            var dataToUpsert = new List<JObject>();
            indexedNodes.ForEach(node => dataToUpsert.AddRange(GetDataToUpsert(new AlgoliaQueueItem(node, AlgoliaTaskType.CREATE, algoliaIndex.IndexName))));
            var searchIndex = await algoliaIndexService.InitializeIndex(algoliaIndex.IndexName, cancellationToken);
            await searchIndex.ReplaceAllObjectsAsync(dataToUpsert, ct: cancellationToken).ConfigureAwait(false);
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
