using DancingGoat.Models;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;

namespace Kentico.Xperience.Algolia.Tests.Base;
internal static class MockDataProvider
{
    public static IndexEventWebPageItemModel WebModel => new(
        itemID: 0,
        itemGuid: new Guid(),
        languageName: CzechLanguageName,
        contentTypeName: ArticlePage.CONTENT_TYPE_NAME,
        name: "Name",
        isSecured: false,
        contentTypeID: 1,
        contentLanguageID: 1,
        websiteChannelName: DefaultChannel,
        webPageItemTreePath: "/",
        order: 0
    );

    public static AlgoliaIndexIncludedPath Path => new("/%")
    {
        ContentTypes = [ArticlePage.CONTENT_TYPE_NAME]
    };


    public static AlgoliaIndex Index => new(
        new AlgoliaConfigurationModel()
        {
            IndexName = DefaultIndex,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<AlgoliaIndexIncludedPath>() { Path }
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
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<AlgoliaIndexIncludedPath>() { Path }
        },
        []
    );
}
