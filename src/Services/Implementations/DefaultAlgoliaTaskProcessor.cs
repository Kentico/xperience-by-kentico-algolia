using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CMS.AutomationEngine.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.MediaLibrary;
using CMS.Websites;
using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    internal class DefaultAlgoliaTaskProcessor : IAlgoliaTaskProcessor
    {
        private readonly IAlgoliaClient algoliaClient;
        private readonly IEventLogService eventLogService;
        private readonly IWebPageUrlRetriever urlRetriever;

        public DefaultAlgoliaTaskProcessor(IAlgoliaClient algoliaClient,
            IEventLogService eventLogService,
            IWebPageUrlRetriever webPageUrlRetriever)
        {
            this.algoliaClient = algoliaClient;
            this.eventLogService = eventLogService;
            this.urlRetriever = webPageUrlRetriever;
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
                    deleteIds.AddRange(GetIdsToDelete(deleteTasks));

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
            var algoliaIndex = IndexStore.Instance.GetIndex(queueItem.IndexName) ?? throw new Exception($"AlgoliaIndex {queueItem.IndexName} not found!");
            var data = await algoliaIndex.AlgoliaIndexingStrategy.MapToAlgoliaJObjecstOrNull(queueItem.IndexedItemModel);

            if (data is null)
            {
                return null;
            }

            foreach (var item in data)
            {
                await AddBaseProperties(queueItem.IndexedItemModel, item);
            }

            return data;
        }

        private async Task AddBaseProperties(IndexedItemModel pageItem, JObject data)
        {
            data["objectID"] = pageItem.WebPageItemGuid.ToString();
            data[nameof(IndexedItemModel.ClassName)] = pageItem.ClassName;
            data[nameof(IndexedItemModel.LanguageCode)] = pageItem.LanguageCode;

            try
            {
                data["Url"] = (await urlRetriever.Retrieve(pageItem.WebPageItemGuid, pageItem.LanguageCode)).RelativePath;
            }
            catch (Exception)
            {
                // Retrieve can throw an exception when processing a page update AlgoliaQueueItem
                // and the page was deleted before the update task has processed. In this case, upsert an
                // empty URL
                data["Url"] = string.Empty;
            }
        }

        private IEnumerable<string> GetIdsToDelete(IEnumerable<AlgoliaQueueItem> deleteTasks)
        {
            return deleteTasks.Select(queueItem => queueItem.IndexedItemModel.WebPageItemGuid.ToString());
        }
    }
}
