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


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaTaskLogger"/> class.
        /// </summary>
        public DefaultAlgoliaTaskLogger(IEventLogService eventLogService) {
            this.eventLogService = eventLogService;
        }


        /// <inheritdoc />
        public void HandleEvent(TreeNode node, string eventName)
        {
            var taskType = GetTaskType(node, eventName);

            // Check crawlers
            foreach (var crawlerId in IndexStore.Instance.GetAllCrawlers())
            {
                var url = DocumentURLProvider.GetAbsoluteUrl(node);
                LogCrawlerTask(new AlgoliaCrawlerQueueItem(crawlerId, url, taskType));
            }

            // Check standard indexes
            if (!node.IsAlgoliaIndexed())
            {
                return;
            }
            foreach (var indexName in IndexStore.Instance.GetAllIndexes().Select(index => index.IndexName))
            {
                if (!node.IsIndexedByIndex(indexName))
                {
                    continue;
                }

                LogIndexTask(new AlgoliaQueueItem(node, taskType, indexName));
            }
        }


        /// <summary>
        /// Logs a single <see cref="AlgoliaCrawlerQueueItem"/>.
        /// </summary>
        /// <param name="task">The task to log.</param>
        private void LogCrawlerTask(AlgoliaCrawlerQueueItem task)
        {
            try
            {
                AlgoliaCrawlerQueueWorker.Current.EnqueueCrawlerQueueItem(task);
            }
            catch (InvalidOperationException ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(LogCrawlerTask), ex);
            }
        }


        /// <summary>
        /// Logs a single <see cref="AlgoliaQueueItem"/>.
        /// </summary>
        /// <param name="task">The task to log.</param>
        private void LogIndexTask(AlgoliaQueueItem task)
        {
            try
            {
                AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(task);
            }
            catch (InvalidOperationException ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(LogIndexTask), ex);
            }
        }


        private AlgoliaTaskType GetTaskType(TreeNode node, string eventName)
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
