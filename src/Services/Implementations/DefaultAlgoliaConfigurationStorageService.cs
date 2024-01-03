
using CMS;
using CMS.DataEngine;
using Kentico.Xperience.Algolia.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Services.Implementations;

public class DefaultAlgoliaConfigurationStorageService : IConfigurationStorageService
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
    public async Task<bool> TryCreateIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;

        if (indexProvider.Get().WhereEquals(nameof(AlgoliaindexitemInfo.IndexName), configuration.IndexName).FirstOrDefault() != default)
        {
            return false;
        }

        var newInfo = new AlgoliaindexitemInfo()
        {
            IndexName = configuration.IndexName,
            ChannelName = configuration.ChannelName,
            StrategyName = configuration.StrategyName
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.AlgoliaIndexItemId;

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaindexedlanguageInfo()
                {
                    languageCode = language,
                    AlgoliaIndexItemId = newInfo.AlgoliaIndexItemId
                };

                languageProvider.Set(languageInfo);
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AlgoliaincludedpathitemInfo()
                {
                    AliasPath = path.AliasPath,
                    AlgoliaIndexItemId = newInfo.AlgoliaIndexItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes is not null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliacontenttypeitemInfo()
                        {
                            ContentTypeName = contentType,
                            AlgoliaIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaIndexItemId = newInfo.AlgoliaIndexItemId
                        };
                        contentPathProvider.Set(contentInfo);
                    }
                }
            }
        }

        return true;
    }

    public Task<AlgoliaConfigurationModel?> GetIndexDataOrNull(int indexId)
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;

        var indexInfo = indexProvider.Get().WithID(indexId).FirstOrDefault();
        if (indexInfo == default)
        {
            return Task.FromResult<AlgoliaConfigurationModel?>(default);
        }

        var paths = pathProvider.Get().WhereEquals(nameof(AlgoliaincludedpathitemInfo.AlgoliaIndexItemId), indexInfo.AlgoliaIndexItemId).ToList();
        var contentTypes = contentPathProvider.Get().WhereEquals(nameof(AlgoliaincludedpathitemInfo.AlgoliaIndexItemId), indexInfo.AlgoliaIndexItemId).ToList();

        return Task.FromResult<AlgoliaConfigurationModel?>(new AlgoliaConfigurationModel()
        {
            ChannelName = indexInfo.ChannelName,
            IndexName = indexInfo.IndexName,
            LanguageNames = languageProvider.Get().WhereEquals(nameof(AlgoliaindexedlanguageInfo.AlgoliaIndexItemId), indexInfo.AlgoliaIndexItemId).Select(x => x.languageCode).ToList(),
            RebuildHook = indexInfo.RebuildHook,
            Id = indexInfo.AlgoliaIndexItemId,
            StrategyName = indexInfo.StrategyName,
            Paths = paths.Select(x => new IncludedPath(x.AliasPath)
            {
                Identifier = x.AlgoliaIncludedPathItemId.ToString(),
                ContentTypes = contentTypes.Where(y => x.AlgoliaIncludedPathItemId == y.AlgoliaIncludedPathItemId).Select(y => y.ContentTypeName).ToArray()
            }).ToList()
        });
    }

    public async Task<List<string>> GetExistingIndexNames() => AlgoliaindexitemInfoProvider.ProviderObject.Get().Select(x => x.IndexName).ToList();

    public async Task<List<int>> GetIndexIds() => AlgoliaindexitemInfoProvider.ProviderObject.Get().Select(x => x.AlgoliaIndexItemId).ToList();

    public async Task<IEnumerable<AlgoliaConfigurationModel>> GetAllIndexData()
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;

        var indexInfos = indexProvider.Get().ToList();
        if (indexInfos == default)
        {
            return [];
        }

        var paths = pathProvider.Get().ToList();
        var contentTypes = contentPathProvider.Get().ToList();
        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(x => new AlgoliaConfigurationModel
        {
            ChannelName = x.ChannelName,
            IndexName = x.IndexName,
            LanguageNames = languages.Where(y => y.AlgoliaIndexItemId == x.AlgoliaIndexItemId).Select(y => y.languageCode).ToList(),
            RebuildHook = x.RebuildHook,
            Id = x.AlgoliaIndexItemId,
            StrategyName = x.StrategyName,
            Paths = paths.Where(y => y.AlgoliaIndexItemId == x.AlgoliaIndexItemId).Select(y => new IncludedPath(y.AliasPath)
            {
                Identifier = y.AlgoliaIncludedPathItemId.ToString(),
                ContentTypes = contentTypes.Where(z => z.AlgoliaIncludedPathItemId == y.AlgoliaIncludedPathItemId).Select(z => z.ContentTypeName).ToArray()
            }).ToList()
        });
    }

    public async Task<bool> TryEditIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;

        configuration.IndexName = RemoveWhitespacesUsingStringBuilder(configuration.IndexName ?? "");

        var indexInfo = indexProvider.Get().WhereEquals(nameof(AlgoliaindexitemInfo.IndexName), configuration.IndexName).FirstOrDefault();

        if (indexInfo == default)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaincludedpathitemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaindexedlanguageInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliacontenttypeitemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));

        indexInfo.ChannelName = configuration.IndexName;
        indexInfo.StrategyName = configuration.StrategyName;
        indexInfo.ChannelName = configuration.ChannelName;

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new AlgoliaindexedlanguageInfo()
                {
                    languageCode = language,
                    AlgoliaIndexItemId = indexInfo.AlgoliaIndexItemId
                };

                languageProvider.Set(languageInfo);
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new AlgoliaincludedpathitemInfo()
                {
                    AliasPath = path.AliasPath,
                    AlgoliaIndexItemId = indexInfo.AlgoliaIndexItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes != null)
                {
                    foreach (string? contentType in path.ContentTypes)
                    {
                        var contentInfo = new AlgoliacontenttypeitemInfo()
                        {
                            ContentTypeName = contentType,
                            AlgoliaIncludedPathItemId = pathInfo.AlgoliaIncludedPathItemId,
                            AlgoliaIndexItemId = indexInfo.AlgoliaIndexItemId
                        };
                        contentPathProvider.Set(contentInfo);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> TryDeleteIndex(int id)
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;

        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaincludedpathitemInfo.AlgoliaIndexItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaindexedlanguageInfo.AlgoliaIndexItemId)} = {id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliacontenttypeitemInfo.AlgoliaIndexItemId)} = {id}"));

        return true;
    }

    public async Task<bool> TryDeleteIndex(AlgoliaConfigurationModel configuration)
    {
        var pathProvider = AlgoliaincludedpathitemInfoProvider.ProviderObject;
        var contentPathProvider = AlgoliacontenttypeitemInfoProvider.ProviderObject;
        var indexProvider = AlgoliaindexitemInfoProvider.ProviderObject;
        var languageProvider = AlgoliaindexedlanguageInfoProvider.ProviderObject;
        indexProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaincludedpathitemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliaindexedlanguageInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        contentPathProvider.BulkDelete(new WhereCondition($"{nameof(AlgoliacontenttypeitemInfo.AlgoliaIndexItemId)} = {configuration.Id}"));
        return true;
    }
}
