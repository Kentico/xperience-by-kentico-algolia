using DancingGoat.Models;

using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;

namespace Kentico.Xperience.Algolia.Tests.Base;

internal static class MockDataProvider
{
    public static IndexEventWebPageItemModel WebModel(IndexEventWebPageItemModel item)
    {
        item.LanguageName = CzechLanguageName;
        item.ContentTypeName = ArticlePage.CONTENT_TYPE_NAME;
        item.Name = "Name";
        item.ContentTypeID = 1;
        item.ContentLanguageID = 1;
        item.WebsiteChannelName = DefaultChannel;
        item.WebPageItemTreePath = "/%";

        return item;
    }

    public static AlgoliaIndexIncludedPath Path => new("/%")
    {
        ContentTypes = [new AlgoliaIndexContentType(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))]
    };


    public static AlgoliaIndex Index => new(
        new AlgoliaConfigurationModel()
        {
            IndexName = DefaultIndex,
            ChannelName = DefaultChannel,
            LanguageNames = [EnglishLanguageName, CzechLanguageName],
            Paths = [Path],
            StrategyName = "strategy"
        },
        []
    );

    public static readonly string DefaultIndex = "SimpleIndex";
    public static readonly string DefaultChannel = "DefaultChannel";
    public static readonly string EnglishLanguageName = "en";
    public static readonly string CzechLanguageName = "cz";
    public static readonly int IndexId = 1;
    public static readonly string EventName = "publish";

    public static AlgoliaIndex GetIndex(string indexName, int id) => new(
        new AlgoliaConfigurationModel()
        {
            Id = id,
            IndexName = indexName,
            ChannelName = DefaultChannel,
            LanguageNames = [EnglishLanguageName, CzechLanguageName],
            Paths = [Path]
        },
        []
    );
}
