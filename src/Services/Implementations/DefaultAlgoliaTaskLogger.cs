using CMS;
using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Collections.Generic;

[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
    {
        private readonly IAlgoliaRegistrationService algoliaRegistrationService;
        private readonly IEventLogService eventLogService;


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
            foreach (var index in algoliaRegistrationService.GetAllIndexes())
            {
                if (!algoliaRegistrationService.IsNodeIndexedByIndex(node, index.IndexName))
                {
                    continue;
                }

                var indexedColumns = algoliaRegistrationService.GetIndexedColumnNames(index.IndexName);
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
                    IndexName = index.IndexName
                });
            }
        }
    }
}
