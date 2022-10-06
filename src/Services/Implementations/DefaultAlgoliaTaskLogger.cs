using System;
using System.Linq;

using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaTaskLogger"/>.
    /// </summary>
    internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
    {
        private readonly IEventLogService eventLogService;


        public DefaultAlgoliaTaskLogger(IEventLogService eventLogService) {
            this.eventLogService = eventLogService;
        }


        /// <inheritdoc />
        public void HandleEvent(TreeNode node, string eventName)
        {
            foreach (var indexName in IndexStore.Instance.GetAll().Select(index => index.IndexName))
            {
                if (!node.IsIndexedByIndex(indexName))
                {
                    continue;
                }

                try
                {
                    var queueItem = new AlgoliaQueueItem(node, GetTaskType(node, eventName), indexName, node.ChangedColumns());
                    AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(queueItem);
                }
                catch (InvalidOperationException ex)
                {
                    eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(HandleEvent), ex);
                }
            }
        }


        private static AlgoliaTaskType GetTaskType(TreeNode node, string eventName)
        {
            if (eventName.Equals(WorkflowEvents.Publish.Name, StringComparison.OrdinalIgnoreCase) && node.WorkflowHistory.Count == 0)
            {
                return AlgoliaTaskType.CREATE;
            }

            if (eventName.Equals(WorkflowEvents.Publish.Name, StringComparison.OrdinalIgnoreCase) && node.WorkflowHistory.Count > 0)
            {
                return AlgoliaTaskType.UPDATE;
            }

            if (eventName.Equals(DocumentEvents.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
                eventName.Equals(WorkflowEvents.Archive.Name, StringComparison.OrdinalIgnoreCase))
            {
                return AlgoliaTaskType.DELETE;
            }

            return AlgoliaTaskType.UNKNOWN;
        }
    }
}
