using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.WorkflowEngine;

using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    internal class DefaultAlgoliaTaskProcessor : IAlgoliaTaskProcessor
    {
        private readonly IAlgoliaClient algoliaClient;
        private readonly IAlgoliaObjectGenerator algoliaObjectGenerator;
        private readonly IEventLogService eventLogService;
        private readonly IWorkflowStepInfoProvider workflowStepInfoProvider;
        private readonly IVersionHistoryInfoProvider versionHistoryInfoProvider;


        public DefaultAlgoliaTaskProcessor(IAlgoliaClient algoliaClient,
            IEventLogService eventLogService,
            IWorkflowStepInfoProvider workflowStepInfoProvider,
            IVersionHistoryInfoProvider versionHistoryInfoProvider,
            IAlgoliaObjectGenerator algoliaObjectGenerator)
        {
            this.algoliaClient = algoliaClient;
            this.eventLogService = eventLogService;
            this.workflowStepInfoProvider = workflowStepInfoProvider;
            this.versionHistoryInfoProvider = versionHistoryInfoProvider;
            this.algoliaObjectGenerator = algoliaObjectGenerator;
        }


        /// <inheritdoc />
        public async Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> queueItems, CancellationToken cancellationToken)
        {
            var successfulOperations = 0;

            // Group queue items based on index name
            var groups = queueItems.GroupBy(item => item.IndexName);
            foreach (var group in groups)
            {
                try
                {
                    var algoliaIndex = IndexStore.Instance.GetIndex(group.Key);

                    var deleteIds = new List<string>();
                    var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE);
                    deleteIds.AddRange(GetIdsToDelete(algoliaIndex, deleteTasks));

                    var updateTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.UPDATE || queueItem.TaskType == AlgoliaTaskType.CREATE);
                    var upsertData = new List<JObject>();
                    foreach (var queueItem in updateTasks)
                    {
                        // There may be less fragments than previously indexed. Delete fragments created by the
                        // previous version of the node
                        deleteIds.AddRange(GetFragmentsToDelete(queueItem.Node, algoliaIndex, queueItem.TaskType));
                        var data = GetDataToUpsert(queueItem.Node, algoliaIndex, queueItem.TaskType);
                        upsertData.AddRange(data);
                    }

                    successfulOperations += await algoliaClient.DeleteRecords(deleteIds, group.Key, cancellationToken);
                    successfulOperations += await algoliaClient.UpsertRecords(upsertData, group.Key, cancellationToken);
                }
                catch (Exception ex)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), ex.Message);
                }
            }

            return successfulOperations;
        }


        /// <inheritdoc/>
        public async Task<int> ProcessCrawlerTasks(IEnumerable<AlgoliaCrawlerQueueItem> queueItems, CancellationToken cancellationToken)
        {
            var successfulOperations = 0;

            // Group queue items based on crawler ID
            var groups = queueItems.GroupBy(item => item.CrawlerId);
            foreach (var group in groups)
            {
                var urlsToUpdate = group
                    .Where(t => t.TaskType.Equals(AlgoliaTaskType.UPDATE) || t.TaskType.Equals(AlgoliaTaskType.CREATE))
                    .Select(t => t.Url);
                if (urlsToUpdate.Any())
                {
                    successfulOperations += await algoliaClient.CrawlUrls(group.Key, urlsToUpdate, cancellationToken);
                }

                var urlsToDelete = group
                    .Where(t => t.TaskType.Equals(AlgoliaTaskType.DELETE))
                    .Select(t => t.Url);
                if (urlsToDelete.Any())
                {
                    successfulOperations += await algoliaClient.DeleteUrls(group.Key, urlsToDelete, cancellationToken);
                }
            }

            return successfulOperations;
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


        /// <summary>
        /// Gets the IDs of the fragments previously generated by a node update. Because the data that was split could
        /// be smaller than previous updates, if they were not deleted during an update task, there would be orphaned
        /// data in Algolia. When the <paramref name="taskType"/> is <see cref="AlgoliaTaskType.UPDATE"/>, we must check
        /// for a previous version and delete the fragments generated by that version, before upserting new fragments.
        /// </summary>
        /// <param name="node">The node to get the previous version of.</param>
        /// <param name="algoliaIndex">The index containing the <paramref name="node"/>.</param>
        /// <param name="taskType">The task type that is being processed.</param>
        /// <returns>A list of Algolia IDs that should be deleted, or an empty list.</returns>
        /// <exception cref="ArgumentNullException" />
        private IEnumerable<string> GetFragmentsToDelete(TreeNode node, AlgoliaIndex algoliaIndex, AlgoliaTaskType taskType)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (algoliaIndex == null)
            {
                throw new ArgumentNullException(nameof(algoliaIndex));
            }

            if (taskType != AlgoliaTaskType.UPDATE || algoliaIndex.DistinctOptions == null)
            {
                // Only split data on UPDATE tasks if splitting is enabled
                return Enumerable.Empty<string>();
            }

            var publishedStepId = workflowStepInfoProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(WorkflowStepInfo.StepWorkflowID), node.WorkflowStep.StepWorkflowID)
                .WhereEquals(nameof(WorkflowStepInfo.StepType), WorkflowStepTypeEnum.DocumentPublished)
                .AsIDQuery()
                .GetScalarResult<int>(0);
            var previouslyPublishedVersionID = versionHistoryInfoProvider.Get()
                .TopN(1)
                .WhereEquals(nameof(VersionHistoryInfo.DocumentID), node.DocumentID)
                .WhereEquals(nameof(VersionHistoryInfo.NodeSiteID), node.NodeSiteID)
                .WhereEquals(nameof(VersionHistoryInfo.VersionWorkflowStepID), publishedStepId)
                .OrderByDescending(nameof(VersionHistoryInfo.WasPublishedTo))
                .AsIDQuery()
                .GetScalarResult<int>(0);
            if (previouslyPublishedVersionID == 0)
            {
                return Enumerable.Empty<string>();
            }

            var previouslyPublishedNode = node.VersionManager.GetVersion(previouslyPublishedVersionID, node);
            return GetDataToUpsert(previouslyPublishedNode, algoliaIndex, AlgoliaTaskType.CREATE).Select(obj => obj.Value<string>("objectID"));
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
    }
}
