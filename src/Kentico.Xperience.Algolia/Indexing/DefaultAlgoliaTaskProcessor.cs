using CMS.Core;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Indexing;

internal class DefaultAlgoliaTaskProcessor : IAlgoliaTaskProcessor
{
    private readonly IAlgoliaClient algoliaClient;
    private readonly IServiceProvider serviceProvider;
    private readonly IEventLogService eventLogService;
    private readonly IWebPageUrlRetriever urlRetriever;

    public DefaultAlgoliaTaskProcessor(IAlgoliaClient algoliaClient,
        IEventLogService eventLogService,
        IWebPageUrlRetriever urlRetriever,
        IServiceProvider serviceProvider)
    {
        this.algoliaClient = algoliaClient;
        this.eventLogService = eventLogService;
        this.serviceProvider = serviceProvider;
        this.urlRetriever = urlRetriever;
    }

    /// <inheritdoc />
    public async Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> queueItems, CancellationToken cancellationToken)
    {
        int successfulOperations = 0;

        // Group queue items based on index name
        var groups = queueItems.GroupBy(item => item.IndexName);
        foreach (var group in groups)
        {
            try
            {
                var deleteIds = new List<string>();
                var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE).ToList();

                var updateTasks = group.Where(queueItem => queueItem.TaskType is AlgoliaTaskType.PUBLISH_INDEX or AlgoliaTaskType.UPDATE);
                var upsertData = new List<JObject>();
                foreach (var queueItem in updateTasks)
                {
                    var documents = await GetDocument(queueItem);
                    if (documents is not null)
                    {
                        foreach (var document in documents)
                        {
                            upsertData.Add(document);
                        }
                    }
                    else
                    {
                        deleteTasks.Add(queueItem);
                    }
                }
                deleteIds.AddRange(GetIdsToDelete(deleteTasks ?? new List<AlgoliaQueueItem>()).Where(x => x is not null).Select(x => x ?? string.Empty));

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
    public async Task<IEnumerable<JObject>?> GetDocument(AlgoliaQueueItem queueItem)
    {
        var algoliaIndex = AlgoliaIndexStore.Instance.GetRequiredIndex(queueItem.IndexName);

        var algoliaStrategy = serviceProvider.GetRequiredStrategy(algoliaIndex);

        var data = await algoliaStrategy.MapToAlgoliaJObjectsOrNull(queueItem.ItemToIndex);

        if (data is null)
        {
            return null;
        }

        foreach (var item in data)
        {
            await AddBaseProperties(queueItem.ItemToIndex, item);
        }

        return data;
    }

    private async Task AddBaseProperties(IIndexEventItemModel item, JObject data)
    {
        data[BaseJObjectProperties.CONTENT_TYPE_NAME] = item.ContentTypeName;
        data[BaseJObjectProperties.LANGUAGE_NAME] = item.LanguageName;
        data[BaseJObjectProperties.ITEM_GUID] = item.ItemGuid;
        data[BaseJObjectProperties.OBJECT_ID] = item.ItemGuid;

        if (item is IndexEventWebPageItemModel webpageItem && string.IsNullOrEmpty((string?)data.GetValue(BaseJObjectProperties.URL)))
        {
            try
            {
                data[BaseJObjectProperties.URL] = (await urlRetriever.Retrieve(webpageItem.WebPageItemTreePath, webpageItem.WebsiteChannelName, webpageItem.LanguageName)).RelativePath;
            }
            catch (Exception)
            {
                // Retrieve can throw an exception when processing a page update AlgoliaQueueItem
                // and the page was deleted before the update task has processed. In this case, upsert an
                // empty URL
                data[BaseJObjectProperties.URL] = string.Empty;
            }
        }
    }

    private static IEnumerable<string?> GetIdsToDelete(IEnumerable<AlgoliaQueueItem> deleteTasks) => deleteTasks.Select(queueItem => queueItem.ItemToIndex.ItemGuid.ToString());
}
