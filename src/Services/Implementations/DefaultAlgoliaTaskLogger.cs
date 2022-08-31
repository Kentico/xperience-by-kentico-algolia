using System;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using static Kentico.Xperience.Algolia.Models.AlgoliaQueueItem;

[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaTaskLogger"/>.
    /// </summary>
    internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
    {
        /// <inheritdoc />
        public void LogTask(AlgoliaQueueItem task)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(task);
        }


        /// <inheritdoc />
        public void LogTasks(IEnumerable<AlgoliaQueueItem> tasks)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItems(tasks);
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

                LogTask(new AlgoliaQueueItem()
                {
                    Node = node,
                    TaskType = GetTaskType(node, eventName),
                    IndexName = indexName
                });
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

            if (eventName.Equals(DocumentCultureDataInfo.TYPEINFO.Events.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
                eventName.Equals(DocumentCultureDataInfo.TYPEINFO.Events.BulkDelete.Name, StringComparison.OrdinalIgnoreCase) ||
                eventName.Equals(WorkflowEvents.Archive.Name, StringComparison.OrdinalIgnoreCase))
            {
                return AlgoliaTaskType.DELETE;
            }

            return AlgoliaTaskType.UNKNOWN;
        }
    }
}
