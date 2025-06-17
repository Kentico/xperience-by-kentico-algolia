using System.Text;

using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

internal class DefaultAlgoliaConfigurationStorageService : IAlgoliaConfigurationStorageService
{
    private readonly IInfoProvider<AlgoliaIndexItemInfo> indexProvider;
    private readonly IInfoProvider<AlgoliaIncludedPathItemInfo> pathProvider;
    private readonly IInfoProvider<AlgoliaContentTypeItemInfo> contentTypeProvider;
    private readonly IInfoProvider<AlgoliaIndexLanguageItemInfo> languageProvider;
    private readonly IInfoProvider<AlgoliaReusableContentTypeItemInfo> reusableContentTypeProvider;

    public DefaultAlgoliaConfigurationStorageService(
        IInfoProvider<AlgoliaIndexItemInfo> indexProvider,
        IInfoProvider<AlgoliaIncludedPathItemInfo> pathProvider,
        IInfoProvider<AlgoliaContentTypeItemInfo> contentTypeProvider,
        IInfoProvider<AlgoliaIndexLanguageItemInfo> languageProvider,
        IInfoProvider<AlgoliaReusableContentTypeItemInfo> reusableContentTypeProvider
    )
    {
        this.indexProvider = indexProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
        this.reusableContentTypeProvider = reusableContentTypeProvider;
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
            AlgoliaIndexItemIndexName = configuration.IndexName ?? string.Empty,
            AlgoliaIndexItemChannelName = configuration.ChannelName ?? string.Empty,
            AlgoliaIndexItemStrategyName = configuration.StrategyName ?? string.Empty,
            AlgoliaIndexItemRebuildHook = configuration.RebuildHook ?? string.Empty
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
                    foreach (var contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliaContentTypeItemInfo()
                        {
                            AlgoliaContentTypeItemContentTypeName = contentType.ContentTypeName,
                            AlgoliaContentTypeItemIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaContentTypeItemIndexItemId = newInfo.AlgoliaIndexItemId
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        if (configuration.ReusableContentTypeNames is not null)
        {
            foreach (string? reusableContentTypeName in configuration.ReusableContentTypeNames)
            {
                var reusableContentTypeItemInfo = new AlgoliaReusableContentTypeItemInfo()
                {
                    AlgoliaReusableContentTypeItemContentTypeName = reusableContentTypeName,
                    AlgoliaReusableContentTypeItemIndexItemId = newInfo.AlgoliaIndexItemId
                };

                reusableContentTypeItemInfo.Insert();
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

        var contentTypesInfoItems = contentTypeProvider
        .Get()
        .WhereEquals(nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId), indexInfo.AlgoliaIndexItemId)
        .GetEnumerableTypedResult();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.AlgoliaContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new AlgoliaIndexContentType(x.ClassName, x.ClassDisplayName));

        var reusableContentTypes = reusableContentTypeProvider.Get().WhereEquals(nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId), indexInfo.AlgoliaIndexItemId).GetEnumerableTypedResult();

        var languages = languageProvider.Get().WhereEquals(nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId), indexInfo.AlgoliaIndexItemId).GetEnumerableTypedResult();

        return new AlgoliaConfigurationModel(indexInfo, languages, paths, contentTypes, reusableContentTypes);
    }


    public List<string> GetExistingIndexNames() => indexProvider.Get().Select(x => x.AlgoliaIndexItemIndexName).ToList();


    public List<int> GetIndexIds() => indexProvider.Get().Select(x => x.AlgoliaIndexItemId).ToList();


    public IEnumerable<AlgoliaConfigurationModel> GetAllIndexData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return [];
        }

        var paths = pathProvider.Get().ToList();

        var contentTypesInfoItems = contentTypeProvider
            .Get()
            .GetEnumerableTypedResult();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.AlgoliaContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new AlgoliaIndexContentType(x.ClassName, x.ClassDisplayName));

        var languages = languageProvider.Get().ToList();

        var reusableContentTypes = reusableContentTypeProvider.Get().ToList();

        return indexInfos.Select(index => new AlgoliaConfigurationModel(index, languages, paths, contentTypes, reusableContentTypes));
    }


    public bool TryEditIndex(AlgoliaConfigurationModel configuration)
    {
        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? string.Empty);

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

        indexInfo.AlgoliaIndexItemRebuildHook = configuration.RebuildHook ?? string.Empty;
        indexInfo.AlgoliaIndexItemStrategyName = configuration.StrategyName ?? string.Empty;
        indexInfo.AlgoliaIndexItemChannelName = configuration.ChannelName ?? string.Empty;
        indexInfo.AlgoliaIndexItemIndexName = configuration.IndexName ?? string.Empty;

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
                    foreach (var contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliaContentTypeItemInfo()
                        {
                            AlgoliaContentTypeItemContentTypeName = contentType.ContentTypeName ?? string.Empty,
                            AlgoliaContentTypeItemIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaContentTypeItemIndexItemId = indexInfo.AlgoliaIndexItemId,
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        RemoveUnusedReusableContentTypes(configuration);
        SetNewIndexReusableContentTypeItems(configuration, indexInfo);

        return true;
    }


    public bool TryDeleteIndex(int id)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId)} = {id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {id}"));
        reusableContentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId)} = {id}"));

        return true;
    }


    public bool TryDeleteIndex(AlgoliaConfigurationModel configuration)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {configuration.Id}"));
        reusableContentTypeProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId)} = {configuration.Id}"));

        return true;
    }


    private void RemoveUnusedReusableContentTypes(AlgoliaConfigurationModel configuration)
    {
        var removeReusableContentTypesQuery = reusableContentTypeProvider
            .Get()
            .WhereEquals(nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId), configuration.Id)
            .WhereNotIn(nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemContentTypeName), configuration.ReusableContentTypeNames.ToArray());

        reusableContentTypeProvider.BulkDelete(new WhereCondition(removeReusableContentTypesQuery));
    }


    private void SetNewIndexReusableContentTypeItems(AlgoliaConfigurationModel configuration, AlgoliaIndexItemInfo indexInfo)
    {
        var newReusableContentTypes = GetNewReusableContentTypesOnIndex(configuration);

        foreach (string? reusableContentType in newReusableContentTypes)
        {
            var reusableContentTypeInfo = new AlgoliaReusableContentTypeItemInfo()
            {
                AlgoliaReusableContentTypeItemContentTypeName = reusableContentType,
                AlgoliaReusableContentTypeItemIndexItemId = indexInfo.AlgoliaIndexItemId,
            };

            reusableContentTypeProvider.Set(reusableContentTypeInfo);
        }
    }


    private IEnumerable<string> GetNewReusableContentTypesOnIndex(AlgoliaConfigurationModel configuration)
    {
        var existingReusableContentTypes = reusableContentTypeProvider
            .Get()
            .WhereEquals(nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId), configuration.Id)
            .GetEnumerableTypedResult();

        return configuration.ReusableContentTypeNames.Where(x => !existingReusableContentTypes.Any(y => y.AlgoliaReusableContentTypeItemContentTypeName == x));
    }
}
