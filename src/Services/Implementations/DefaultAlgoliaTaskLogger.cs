using CMS;
using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Collections.Generic;
using System.Linq;

[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaTaskLogger"/>.
    /// </summary>
    internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
    {
        private readonly IAlgoliaRegistrationService algoliaRegistrationService;
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaTaskLogger"/> class.
        /// </summary>
        public DefaultAlgoliaTaskLogger(IAlgoliaRegistrationService algoliaRegistrationService, IEventLogService eventLogService)
        {
            this.algoliaRegistrationService = algoliaRegistrationService;
            this.eventLogService = eventLogService;
        }


        public void LogTask(AlgoliaQueueItem task)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(task);
        }


        public void LogTasks(IEnumerable<AlgoliaQueueItem> tasks)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItems(tasks);
        }


        public void HandleEvent(TreeNode node, string eventName)
        {
            foreach (var indexName in algoliaRegistrationService.GetAllIndexes().Select(index => index.IndexName))
            {
                if (!algoliaRegistrationService.IsNodeIndexedByIndex(node, indexName))
                {
                    continue;
                }

                var indexedColumns = algoliaRegistrationService.GetIndexedColumnNames(indexName);
                if (indexedColumns.Length == 0)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaTaskLogger), nameof(LogTasks), $"Unable to enqueue node change: Error loading indexed columns.");
                    continue;
                }

                if (eventName.Equals(WorkflowEvents.Publish.Name) && !node.AnyItemChanged(indexedColumns))
                {
                    // for Publish event, don't update Algolia if nothing changed
                    continue;
                }

                var shouldDelete = eventName.Equals(DocumentEvents.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
                    eventName.Equals(WorkflowEvents.Archive.Name, StringComparison.OrdinalIgnoreCase);

                LogTask(new AlgoliaQueueItem()
                {
                    Node = node,
                    Delete = shouldDelete,
                    IndexName = indexName
                });
            }
        }
    }
}
