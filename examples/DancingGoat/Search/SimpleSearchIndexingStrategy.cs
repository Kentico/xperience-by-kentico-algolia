using CMS.ContentEngine;
using CMS.Websites;
using Kentico.Xperience.Algolia.Indexing;
using Newtonsoft.Json.Linq;
using DancingGoat.Models;
using Microsoft.IdentityModel.Tokens;
using Algolia.Search.Models.Settings;
using DancingGoat.Search.Models;

namespace DancingGoat.Search;

public class SimpleSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public SimpleSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override IndexSettings GetAlgoliaIndexSettings() =>
        new()
        {
            AttributesToRetrieve = new List<string>
            {
                nameof(DancingGoatSimpleSearchResultModel.Title)
            }
        };

    public override async Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        var result = new List<JObject>();

        string title = "";

        // IIndexEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (algoliaPageItem is IndexEventWebPageItemModel indexedPage)
        {
            if (string.Equals(algoliaPageItem.ContentTypeName, HomePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                var page = await GetPage<HomePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    HomePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                if (page.HomePageBanner.IsNullOrEmpty())
                {
                    return null;
                }

                title = page!.HomePageBanner.First().BannerHeaderText;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        var jObject = new JObject();
        jObject[nameof(DancingGoatSimpleSearchResultModel.Title)] = title;

        result.Add(jObject);

        return result;
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config =>
                    config
                        .WithLinkedItems(4) // You could parameterize this if you want to optimize specific database queries
                        .ForWebsite(channelName)
                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), id))
                        .TopN(1))
            .InLanguage(languageName);

        var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<T>);

        return result.FirstOrDefault();
    }
}
