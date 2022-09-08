using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;

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
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IAlgoliaObjectGenerator algoliaObjectGenerator,
            IEventLogService eventLogService)
        {
            this.algoliaIndexService = algoliaIndexService;
            this.algoliaObjectGenerator = algoliaObjectGenerator;
            this.eventLogService = eventLogService;
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
                    var updateTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.UPDATE || queueItem.TaskType == AlgoliaTaskType.CREATE);
                    var upsertData = new List<JObject>();
                    foreach (var queueItem in updateTasks)
                    {
                        var data = GetDataToUpsert(queueItem.Node, algoliaIndex, queueItem.TaskType);
                        upsertData.AddRange(data);
                    }

                    var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE);
                    var deleteIds = GetIdsToDelete(algoliaIndex, deleteTasks);

                    successfulOperations += await UpsertRecords(upsertData, group.Key, cancellationToken);
                    successfulOperations += await DeleteRecords(deleteIds, group.Key, cancellationToken);
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


        private IEnumerable<JObject> GetDataToUpsert(TreeNode node, AlgoliaIndex algoliaIndex, AlgoliaTaskType taskType)
        {
            if (algoliaIndex.DistinctOptions != null)
            {
                // If the data is split, force CREATE type to push all data to Algolia
                var nodeData = algoliaObjectGenerator.GetTreeNodeData(node, algoliaIndex.Type, AlgoliaTaskType.CREATE);
                return algoliaObjectGenerator.SplitData(nodeData, algoliaIndex);
            }

            return new JObject[] { algoliaObjectGenerator.GetTreeNodeData(node, algoliaIndex.Type, taskType) };
        }


        private IEnumerable<string> GetIdsToDelete(AlgoliaIndex algoliaIndex, IEnumerable<AlgoliaQueueItem> deleteTasks)
        {
            if (algoliaIndex.DistinctOptions != null)
            {
                // Data has been split, get IDs of the smaller records
                var ids = new List<string>();
                foreach (var queueItem in deleteTasks)
                {
                    var data = GetDataToUpsert(queueItem.Node, algoliaIndex, queueItem.TaskType);
                    ids.AddRange(data.Select(obj => obj.Value<string>("objectID")));
                }

                return ids;
            }

            return deleteTasks.Select(queueItem => queueItem.Node.DocumentID.ToString());
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

            var dataToUpsert = new List<JObject>();
            indexedNodes.ForEach(node => dataToUpsert.AddRange(GetDataToUpsert(node, algoliaIndex, AlgoliaTaskType.CREATE)));
            var searchIndex = algoliaIndexService.InitializeIndex(algoliaIndex.IndexName);
            await searchIndex.ReplaceAllObjectsAsync(dataToUpsert).ConfigureAwait(false);
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
