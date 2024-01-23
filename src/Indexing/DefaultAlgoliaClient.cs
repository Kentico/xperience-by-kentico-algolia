using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using CMS.Base.Internal;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;
using CMS.Websites;
using Kentico.Xperience.Algolia.Services;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Default implementation of <see cref="IAlgoliaClient"/>.
/// </summary>
internal class DefaultAlgoliaClient : IAlgoliaClient
{
    private readonly IAlgoliaIndexService algoliaIndexService;
    private readonly IInfoProvider<ContentLanguageInfo> languageProvider;
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IConversionService conversionService;
    private readonly ICacheAccessor cacheAccessor;
    private readonly IContentQueryExecutor executor;
    private readonly IProgressiveCache cache;
    private readonly ISearchClient searchClient;

    internal const string CACHEKEY_STATISTICS = "Algolia|ListIndices";

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
    /// </summary>
    public DefaultAlgoliaClient(
        IAlgoliaIndexService algoliaIndexService,
        ICacheAccessor cacheAccessor,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        ISearchClient searchClient,
        IContentQueryExecutor executor)
    {
        this.algoliaIndexService = algoliaIndexService;
        this.cacheAccessor = cacheAccessor;
        this.cache = cache;
        this.searchClient = searchClient;
        this.executor = executor;
        this.languageProvider = languageProvider;
        this.channelProvider = channelProvider;
        this.conversionService = conversionService;
    }

    /// <inheritdoc />
    public Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (objectIds == null || !objectIds.Any())
        {
            return Task.FromResult(0);
        }

        return DeleteRecordsInternal(objectIds, indexName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ICollection<IndicesResponse>> GetStatistics(CancellationToken cancellationToken) =>
        (await searchClient.ListIndicesAsync(ct: cancellationToken).ConfigureAwait(false)).Items;

    /// <inheritdoc />
    public Task Rebuild(string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        var algoliaIndex = AlgoliaIndexStore.Instance.GetIndex(indexName);
        if (algoliaIndex == null)
        {
            throw new InvalidOperationException($"The index '{indexName}' is not registered.");
        }

        return RebuildInternal(algoliaIndex, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (dataObjects == null || !dataObjects.Any())
        {
            return Task.FromResult(0);
        }

        return UpsertRecordsInternal(dataObjects, indexName, cancellationToken);
    }

    private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
    {
        var deletedCount = 0;
        var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
        var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(objectIds, ct: cancellationToken).ConfigureAwait(false);
        foreach (var response in batchIndexingResponse.Responses)
        {
            deletedCount += response.ObjectIDs.Count();
        }

        return deletedCount;
    }

    private async Task RebuildInternal(AlgoliaIndex algoliaIndex, CancellationToken cancellationToken)
    {
        // Clear statistics cache so listing displays updated data after rebuild
        cacheAccessor.Remove(CACHEKEY_STATISTICS);

        var indexedItems = new List<IndexEventWebPageItemModel>();
        foreach (var includedPathAttribute in algoliaIndex.IncludedPaths)
        {
            foreach (var language in algoliaIndex.LanguageNames)
            {
                var queryBuilder = new ContentItemQueryBuilder();

                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    foreach (var contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilder.ForContentType(contentType, config => config.WithLinkedItems(1).ForWebsite(algoliaIndex.WebSiteChannelName, includeUrlPath: true));
                    }
                }
                queryBuilder.InLanguage(language);

                var webpages = await executor.GetWebPageResult(queryBuilder, container => container, cancellationToken: cancellationToken);

                foreach (var page in webpages)
                {
                    var item = await MapToEventItem(page);
                    indexedItems.Add(item);
                }
            }
        }

        var searchIndex = await algoliaIndexService.InitializeIndex(algoliaIndex.IndexName, cancellationToken);
        await searchIndex.ClearObjectsAsync(ct: cancellationToken);

        indexedItems.ForEach(node => AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(new AlgoliaQueueItem(node, AlgoliaTaskType.PUBLISH_INDEX, algoliaIndex.IndexName)));
    }

    private async Task<IndexEventWebPageItemModel> MapToEventItem(IWebPageContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? "";

        var websiteChannels = await GetAllWebsiteChannels();

        string channelName = websiteChannels.FirstOrDefault(c => c.WebsiteChannelID == content.WebPageItemWebsiteChannelID).ChannelName ?? "";

        var item = new IndexEventWebPageItemModel(
            content.WebPageItemID,
            content.WebPageItemGUID,
            languageName,
            content.ContentTypeName,
            content.WebPageItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID,
            channelName,
            content.WebPageItemTreePath,
            content.WebPageItemParentID,
            content.WebPageItemOrder);

        return item;
    }

    private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
    {
        var upsertedCount = 0;
        var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
        var batchIndexingResponse = await searchIndex.PartialUpdateObjectsAsync(dataObjects, createIfNotExists: true, ct: cancellationToken).ConfigureAwait(false);
        foreach (var response in batchIndexingResponse.Responses)
        {
            upsertedCount += response.ObjectIDs.Count();
        }

        return upsertedCount;
    }

    private Task<IEnumerable<ContentLanguageInfo>> GetAllLanguages() =>
        cache.LoadAsync(async cs =>
        {
            var results = await languageProvider.Get().GetEnumerableTypedResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");

            return results;
        }, new CacheSettings(5, nameof(DefaultAlgoliaClient), nameof(GetAllLanguages)));

    private Task<IEnumerable<(int WebsiteChannelID, string ChannelName)>> GetAllWebsiteChannels() =>
        cache.LoadAsync(async cs =>
        {

            var results = await channelProvider.Get()
                .Source(s => s.Join<WebsiteChannelInfo>(nameof(ChannelInfo.ChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
                .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(ChannelInfo.ChannelName))
                .GetDataContainerResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { $"{ChannelInfo.OBJECT_TYPE}|all", $"{WebsiteChannelInfo.OBJECT_TYPE}|all" });

            var items = new List<(int WebsiteChannelID, string ChannelName)>();

            foreach (var item in results)
            {
                if (item.TryGetValue(nameof(WebsiteChannelInfo.WebsiteChannelID), out object channelID) && item.TryGetValue(nameof(ChannelInfo.ChannelName), out object channelName))
                {
                    items.Add(new(conversionService.GetInteger(channelID, 0), conversionService.GetString(channelName, "")));
                }
            }

            return items.AsEnumerable();
        }, new CacheSettings(5, nameof(DefaultAlgoliaClient), nameof(GetAllWebsiteChannels)));
}
