using CMS.Core;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Default implementation of <see cref="IAlgoliaTaskLogger"/>.
/// </summary>
internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
{
    private readonly IEventLogService eventLogService;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAlgoliaTaskLogger"/> class.
    /// </summary>
    public DefaultAlgoliaTaskLogger(IEventLogService eventLogService, IServiceProvider serviceProvider)
    {
        this.eventLogService = eventLogService;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task HandleEvent(IndexEventWebPageItemModel webpageItem, string eventName)
    {
        var taskType = GetTaskType(eventName);

        foreach (var algoliaIndex in AlgoliaIndexStore.Instance.GetAllIndices())
        {
            if (!webpageItem.IsIndexedByIndex(eventLogService, algoliaIndex.IndexName, eventName))
            {
                continue;
            }

            var algoliaStrategy = serviceProvider.GetRequiredStrategy(algoliaIndex);

            if (algoliaIndex is not null)
            {
                var toReindex = await algoliaStrategy.FindItemsToReindex(webpageItem);

                if (toReindex is not null)
                {
                    foreach (var item in toReindex)
                    {
                        if (item.ItemGuid == webpageItem.ItemGuid)
                        {
                            if (taskType == AlgoliaTaskType.DELETE)
                            {
                                LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.DELETE, algoliaIndex.IndexName));
                            }
                            else
                            {
                                LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.UPDATE, algoliaIndex.IndexName));
                            }
                        }
                    }
                }
            }
        }
    }

    public async Task HandleReusableItemEvent(IndexEventReusableItemModel reusableItem, string eventName)
    {
        foreach (var algoliaIndex in AlgoliaIndexStore.Instance.GetAllIndices())
        {
            if (!reusableItem.IsIndexedByIndex(eventLogService, algoliaIndex.IndexName, eventName))
            {
                continue;
            }

            var strategy = serviceProvider.GetRequiredStrategy(algoliaIndex);
            var toReindex = await strategy.FindItemsToReindex(reusableItem);

            if (toReindex is not null)
            {
                foreach (var item in toReindex)
                {
                    LogIndexTask(new AlgoliaQueueItem(item, AlgoliaTaskType.UPDATE, algoliaIndex.IndexName));
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
