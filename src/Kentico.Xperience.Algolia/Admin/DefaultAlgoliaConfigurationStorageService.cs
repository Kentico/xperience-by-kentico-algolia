using CMS.DataEngine;
using System.Text;

namespace Kentico.Xperience.Algolia.Admin;

internal class DefaultAlgoliaConfigurationStorageService : IAlgoliaConfigurationStorageService
{
    private readonly IAlgoliaIndexItemInfoProvider indexProvider;
    private readonly IAlgoliaIncludedPathItemInfoProvider pathProvider;
    private readonly IAlgoliaContentTypeItemInfoProvider contentTypeProvider;
    private readonly IAlgoliaIndexLanguageItemInfoProvider languageProvider;

    public DefaultAlgoliaConfigurationStorageService(
        IAlgoliaIndexItemInfoProvider indexProvider,
        IAlgoliaIncludedPathItemInfoProvider pathProvider,
        IAlgoliaContentTypeItemInfoProvider contentTypeProvider,
        IAlgoliaIndexLanguageItemInfoProvider languageProvider
    )
    {
        this.indexProvider = indexProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
    }

    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }
    public bool TryCreateIndex(AlgoliaConfigurationModel configuration)
    {
        var existingIndex = indexProvider.Get()
            .WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), configuration.IndexName)
            .TopN(1)
            .FirstOrDefault();

        if (existingIndex is not null)
        {
            return false;
        }

        var newInfo = new AlgoliaIndexItemInfo()
        {
            AlgoliaIndexItemIndexName = configuration.IndexName ?? "",
            AlgoliaIndexItemChannelName = configuration.ChannelName ?? "",
            AlgoliaIndexItemStrategyName = configuration.StrategyName ?? "",
            AlgoliaIndexItemRebuildHook = configuration.RebuildHook ?? ""
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.AlgoliaIndexItemId;

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaIndexLanguageItemInfo()
                {
                    AlgoliaIndexLanguageItemName = language,
                    AlgoliaIndexLanguageItemIndexItemId = newInfo.AlgoliaIndexItemId
                };

                languageInfo.Insert();
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AlgoliaIncludedPathItemInfo()
                {
                    AlgoliaIncludedPathItemAliasPath = path.AliasPath,
                    AlgoliaIncludedPathItemIndexItemId = newInfo.AlgoliaIndexItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes is not null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliaContentTypeItemInfo()
                        {
                            AlgoliaContentTypeItemContentTypeName = contentType,
                            AlgoliaContentTypeItemIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaContentTypeItemIndexItemId = newInfo.AlgoliaIndexItemId
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }
    public AlgoliaConfigurationModel? GetIndexDataOrNull(int indexId)
    {
        var indexInfo = indexProvider.Get().WithID(indexId).FirstOrDefault();
        if (indexInfo == default)
        {
            return default;
        }

        var paths = pathProvider.Get().WhereEquals(nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId), indexInfo.AlgoliaIndexItemId).GetEnumerableTypedResult();
        var contentTypes = contentTypeProvider.Get().WhereEquals(nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId), indexInfo.AlgoliaIndexItemId).GetEnumerableTypedResult();
        var languages = languageProvider.Get().WhereEquals(nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId), indexInfo.AlgoliaIndexItemId).GetEnumerableTypedResult();

        return new AlgoliaConfigurationModel(indexInfo, languages, paths, contentTypes);
    }
    public List<string> GetExistingIndexNames() => indexProvider.Get().Select(x => x.AlgoliaIndexItemIndexName).ToList();
    public List<int> GetIndexIds() => indexProvider.Get().Select(x => x.AlgoliaIndexItemId).ToList();
    public IEnumerable<AlgoliaConfigurationModel> GetAllIndexData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return new List<AlgoliaConfigurationModel>();
        }

        var paths = pathProvider.Get().ToList();
        var contentTypes = contentTypeProvider.Get().ToList();
        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(index => new AlgoliaConfigurationModel(index, languages, paths, contentTypes));
    }
    public bool TryEditIndex(AlgoliaConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? "");

        var indexInfo = indexProvider.Get()
            .WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        if (indexInfo is null)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {configuration.Id}"));

        indexInfo.AlgoliaIndexItemRebuildHook = configuration.RebuildHook ?? "";
        indexInfo.AlgoliaIndexItemStrategyName = configuration.StrategyName ?? "";
        indexInfo.AlgoliaIndexItemChannelName = configuration.ChannelName ?? "";
        indexInfo.AlgoliaIndexItemIndexName = configuration.IndexName ?? "";

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaIndexLanguageItemInfo()
                {
                    AlgoliaIndexLanguageItemName = language,
                    AlgoliaIndexLanguageItemIndexItemId = indexInfo.AlgoliaIndexItemId,
                };

                languageProvider.Set(languageInfo);
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AlgoliaIncludedPathItemInfo()
                {
                    AlgoliaIncludedPathItemAliasPath = path.AliasPath,
                    AlgoliaIncludedPathItemIndexItemId = indexInfo.AlgoliaIndexItemId,
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes != null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliaContentTypeItemInfo()
                        {
                            AlgoliaContentTypeItemContentTypeName = contentType ?? "",
                            AlgoliaContentTypeItemIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaContentTypeItemIndexItemId = indexInfo.AlgoliaIndexItemId,
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }
    public bool TryDeleteIndex(int id)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId)} = {id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {id}"));

        return true;
    }
    public bool TryDeleteIndex(AlgoliaConfigurationModel configuration)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {configuration.Id}"));

        return true;
    }
}
