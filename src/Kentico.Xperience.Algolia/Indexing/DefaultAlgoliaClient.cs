using Algolia.Search.Clients;
using Algolia.Search.Models.Common;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using Kentico.Xperience.Algolia.Admin;
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
    private readonly IContentQueryExecutor executor;
    private readonly IProgressiveCache cache;
    private readonly ISearchClient searchClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
    /// </summary>
    public DefaultAlgoliaClient(
        IAlgoliaIndexService algoliaIndexService,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        ISearchClient searchClient,
        IContentQueryExecutor executor)
    {
        this.algoliaIndexService = algoliaIndexService;
        this.cache = cache;
        this.searchClient = searchClient;
        this.executor = executor;
        this.languageProvider = languageProvider;
        this.channelProvider = channelProvider;
        this.conversionService = conversionService;
    }

    /// <inheritdoc />
    public Task<int> DeleteRecords(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        if (itemGuids == null || !itemGuids.Any())
        {
            return Task.FromResult(0);
        }

        return DeleteRecordsInternal(itemGuids, indexName, cancellationToken);
    }

    /// <inheritdoc/>
    private async Task<ICollection<IndicesResponse>> GetStatisticsInternal(CancellationToken cancellationToken) =>
        (await searchClient.ListIndicesAsync(ct: cancellationToken).ConfigureAwait(false)).Items;

    /// <inheritdoc/>
    public async Task<ICollection<AlgoliaIndexStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken) =>
        (await GetStatisticsInternal(cancellationToken))
        .Select(i => new AlgoliaIndexStatisticsViewModel
        {
            Name = i.Name,
            Entries = i.Entries,
            UpdatedAt = i.UpdatedAt
        })
        .ToList();

    /// <inheritdoc />
    public Task Rebuild(string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        var algoliaIndex = AlgoliaIndexStore.Instance.GetRequiredIndex(indexName);

        return RebuildInternal(algoliaIndex, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteIndex(string indexName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }

        var searchIndex = searchClient.InitIndex(indexName);

        await searchIndex.DeleteAsync(ct: cancellationToken);
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
        int deletedCount = 0;
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
        var indexedItems = new List<IIndexEventItemModel>();
        foreach (var includedPathAttribute in algoliaIndex.IncludedPaths)
        {
            var pathMatch =
                includedPathAttribute.AliasPath.EndsWith("/%", StringComparison.OrdinalIgnoreCase)
                    ? PathMatch.Children(includedPathAttribute.AliasPath[..^2])
                    : PathMatch.Single(includedPathAttribute.AliasPath);

            foreach (string language in algoliaIndex.LanguageNames)
            {
                var queryBuilder = new ContentItemQueryBuilder();

                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    foreach (var contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilder.ForContentType(contentType.ContentTypeName, config => config.ForWebsite(algoliaIndex.WebSiteChannelName, includeUrlPath: true, pathMatch: pathMatch));
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
        }

        foreach (string language in algoliaIndex.LanguageNames)
        {
            var queryBuilder = new ContentItemQueryBuilder();

            if (algoliaIndex.IncludedReusableContentTypes != null && algoliaIndex.IncludedReusableContentTypes.Count > 0)
            {
                foreach (string reusableContentType in algoliaIndex.IncludedReusableContentTypes)
                {
                    queryBuilder.ForContentType(reusableContentType);
                }

                queryBuilder.InLanguage(language);

                var reusableItems = await executor.GetResult(queryBuilder, result => result, cancellationToken: cancellationToken);

                foreach (var reusableItem in reusableItems)
                {
                    var item = await MapToEventReusableItem(reusableItem);
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

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? string.Empty;

        var websiteChannels = await GetAllWebsiteChannels();

        string channelName = websiteChannels.FirstOrDefault(c => c.WebsiteChannelID == content.WebPageItemWebsiteChannelID).ChannelName ?? string.Empty;

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
            content.WebPageItemOrder);

        return item;
    }

    private async Task<IndexEventReusableItemModel> MapToEventReusableItem(IContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? string.Empty;

        var item = new IndexEventReusableItemModel(
            content.ContentItemID,
            content.ContentItemGUID,
            languageName,
            content.ContentTypeName,
            content.ContentItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID);

        return item;
    }

    private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
    {
        int upsertedCount = 0;
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
                    items.Add(new(conversionService.GetInteger(channelID, 0), conversionService.GetString(channelName, string.Empty)));
                }
            }

            return items.AsEnumerable();
        }, new CacheSettings(5, nameof(DefaultAlgoliaClient), nameof(GetAllWebsiteChannels)));
}
