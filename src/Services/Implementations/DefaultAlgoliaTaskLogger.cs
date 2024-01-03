using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.Core;
using CMS.Websites;
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
        public DefaultAlgoliaTaskLogger(IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
        }


        /// <inheritdoc />
        public async Task HandleEvent(IndexedItemModel indexedItem, string eventName)
        {
            var taskType = GetTaskType(eventName);

            if (!indexedItem.IsAlgoliaIndexed(eventName))
            {
                return;
            }

            foreach (string? indexName in IndexStore.Instance.GetAllIndices().Select(index => index.IndexName))
            {
                if (!indexedItem.IsIndexedByIndex(indexName, eventName))
                {
                    continue;
                }

                var algoliaIndex = IndexStore.Instance.GetIndex(indexName);

                if (algoliaIndex is not null)
                {
                    var toReindex = await algoliaIndex.AlgoliaIndexingStrategy.FindItemsToReindex(indexedItem);

                    if (toReindex is not null)
                    {
                        foreach (var item in toReindex)
                        {
                            if (item.WebPageItemGuid == indexedItem.WebPageItemGuid)
                            {
                                if (taskType == AlgoliaTaskType.DELETE)
                                {
                                    LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.DELETE, indexName));
                                }
                                else
                                {
                                    LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.UPDATE, indexName));
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task HandleContentItemEvent(IndexedContentItemModel indexedItem, string eventName)
        {
            if (!indexedItem.IsAlgoliaIndexed(eventName))
            {
                return;
            }

            foreach (string? indexName in IndexStore.Instance.GetAllIndices().Select(index => index.IndexName))
            {
                if (!indexedItem.IsIndexedByIndex(indexName, eventName))
                {
                    continue;
                }

                var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
                if (algoliaIndex is not null)
                {
                    var toReindex = await algoliaIndex.AlgoliaIndexingStrategy.FindItemsToReindex(indexedItem, algoliaIndex.WebSiteChannelName);
                    if (toReindex is not null)
                    {
                        foreach (var item in toReindex)
                        {
                            LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.UPDATE, indexName));
                        }
                    }
                }
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
                AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(task);
            }
            catch (InvalidOperationException ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaTaskLogger), nameof(LogIndexTask), ex);
            }
        }


        private static AlgoliaTaskType GetTaskType(string eventName)
        {
            if (eventName.Equals(WebPageEvents.Publish.Name, StringComparison.OrdinalIgnoreCase))
            {
                return AlgoliaTaskType.UPDATE;
            }

            if (eventName.Equals(WebPageEvents.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
                eventName.Equals(WebPageEvents.Archive.Name, StringComparison.OrdinalIgnoreCase))
            {
                return AlgoliaTaskType.DELETE;
            }

            return AlgoliaTaskType.UNKNOWN;
        }
    }
}
