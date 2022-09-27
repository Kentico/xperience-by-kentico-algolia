using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
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
        public void LogTask(AlgoliaQueueItem task)
        {
            try
            {
                AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(task);
            }
            catch (InvalidOperationException ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(LogTask), ex);
            }
            
        }


        /// <inheritdoc />
        public void LogTasks(IEnumerable<AlgoliaQueueItem> tasks)
        {
            try
            {
                AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItems(tasks);
            }
            catch (InvalidOperationException ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(LogTasks), ex);
            }
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

                LogTask(new AlgoliaQueueItem(node, GetTaskType(node, eventName), indexName));
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
