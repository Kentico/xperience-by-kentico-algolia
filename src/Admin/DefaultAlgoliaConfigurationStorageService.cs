using CMS.DataEngine;
using Kentico.Xperience.Algolia.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin;

public class DefaultAlgoliaConfigurationStorageService : IAlgoliaConfigurationStorageService
{
    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
                builder.Append(c);
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }
    public bool TryCreateIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;

        if (indexProvider.Get().WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), configuration.IndexName).FirstOrDefault() != default)
        {
            return false;
        }

        var newInfo = new AlgoliaIndexItemInfo()
        {
            AlgoliaIndexItemIndexName = configuration.IndexName,
            AlgoliaIndexItemChannelName = configuration.ChannelName,
            AlgoliaIndexItemStrategyName = configuration.StrategyName
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.AlgoliaIndexItemId;

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaIndexedLanguageInfo()
                {
                    AlgoliaIndexedLanguageName = language,
                    AlgoliaIndexedLanguageIndexItemId = newInfo.AlgoliaIndexItemId
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
                    AlgoliaIncludedPathAliasPath = path.AliasPath,
                    AlgoliaIncludedPathIndexItemId = newInfo.AlgoliaIndexItemId
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
                        contentPathProvider.Set(contentInfo);
                    }
                }
            }
        }

        return true;
    }
    public AlgoliaConfigurationModel? GetIndexDataOrNull(int indexId)
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;

        var indexInfo = indexProvider.Get().WithID(indexId).FirstOrDefault();
        if (indexInfo == default)
        {
            return default;
        }

        var paths = pathProvider.Get().WhereEquals(nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId), indexInfo.AlgoliaIndexItemId).ToList();
        var contentTypes = contentPathProvider.Get().WhereEquals(nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId), indexInfo.AlgoliaIndexItemId).ToList();

        return new AlgoliaConfigurationModel()
        {
            ChannelName = indexInfo.AlgoliaIndexItemChannelName,
            IndexName = indexInfo.AlgoliaIndexItemIndexName,
            LanguageNames = languageProvider.Get().WhereEquals(nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageIndexItemId), indexInfo.AlgoliaIndexItemId).Select(x => x.AlgoliaIndexedLanguageName).ToList(),
            RebuildHook = indexInfo.AlgoliaIndexItemRebuildHook,
            Id = indexInfo.AlgoliaIndexItemId,
            StrategyName = indexInfo.AlgoliaIndexItemStrategyName,
            Paths = paths.Select(x => new AlgoliaIndexIncludedPath(x.AlgoliaIncludedPathAliasPath)
            {
                Identifier = x.AlgoliaIncludedPathItemId.ToString(),
                ContentTypes = contentTypes.Where(y => x.AlgoliaIncludedPathItemId == y.AlgoliaContentTypeItemIncludedPathItemId).Select(y => y.AlgoliaContentTypeItemContentTypeName).ToArray()
            }).ToList()
        };
    }
    public List<string> GetExistingIndexNames() => AlgoliaIndexItemInfoProvider.ProviderObject.Get().Select(x => x.AlgoliaIndexItemIndexName).ToList();
    public List<int> GetIndexIds() => AlgoliaIndexItemInfoProvider.ProviderObject.Get().Select(x => x.AlgoliaIndexItemId).ToList();
    public IEnumerable<AlgoliaConfigurationModel> GetAllIndexData()
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;

        var indexInfos = indexProvider.Get().ToList();
        if (indexInfos == default)
        {
            return new List<AlgoliaConfigurationModel>();
        }

        var paths = pathProvider.Get().ToList();
        var contentTypes = contentPathProvider.Get().ToList();
        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(x => new AlgoliaConfigurationModel
        {
            ChannelName = x.AlgoliaIndexItemChannelName,
            IndexName = x.AlgoliaIndexItemIndexName,
            LanguageNames = languages.Where(y => y.AlgoliaIndexedLanguageIndexItemId == x.AlgoliaIndexItemId).Select(y => y.AlgoliaIndexedLanguageName).ToList(),
            RebuildHook = x.AlgoliaIndexItemRebuildHook,
            Id = x.AlgoliaIndexItemId,
            StrategyName = x.AlgoliaIndexItemStrategyName,
            Paths = paths.Where(y => y.AlgoliaIncludedPathIndexItemId == x.AlgoliaIndexItemId).Select(y => new AlgoliaIndexIncludedPath(y.AlgoliaIncludedPathAliasPath)
            {
                Identifier = y.AlgoliaIncludedPathItemId.ToString(),
                ContentTypes = contentTypes.Where(z => z.AlgoliaContentTypeItemIncludedPathItemId == y.AlgoliaIncludedPathItemId).Select(z => z.AlgoliaContentTypeItemContentTypeName).ToArray()
            }).ToList()
        });
    }
    public bool TryEditIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;

        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? "");

        var indexInfo = indexProvider.Get().WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), configuration.IndexName).FirstOrDefault();

        if (indexInfo == default)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageIndexItemId)} = {configuration.Id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {configuration.Id}"));

        indexInfo.AlgoliaIndexItemChannelName = configuration.IndexName;
        indexInfo.AlgoliaIndexItemStrategyName = configuration.StrategyName;
        indexInfo.AlgoliaIndexItemChannelName = configuration.ChannelName;

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaIndexedLanguageInfo()
                {
                    AlgoliaIndexedLanguageName = language,
                    AlgoliaIndexedLanguageIndexItemId = indexInfo.AlgoliaIndexItemId
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
                    AlgoliaIncludedPathAliasPath = path.AliasPath,
                    AlgoliaIncludedPathIndexItemId = indexInfo.AlgoliaIndexItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes != null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliaContentTypeItemInfo()
                        {
                            AlgoliaContentTypeItemContentTypeName = contentType,
                            AlgoliaContentTypeItemIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaContentTypeItemIndexItemId = indexInfo.AlgoliaIndexItemId
                        };
                        contentPathProvider.Set(contentInfo);
                    }
                }
            }
        }

        return true;
    }
    public bool TryDeleteIndex(int id)
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;

        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageIndexItemId)} = {id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {id}"));

        return true;
    }
    public bool TryDeleteIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaIncludedPathItemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliaContentTypeItemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaIndexItemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaIndexedLanguageInfoProvider.ProviderObject;
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageIndexItemId)} = {configuration.Id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId)} = {configuration.Id}"));
        return true;
    }
}
