using Algolia.Search.Models.Settings;
using CMS.ContentEngine;
using CMS.Websites;
using DancingGoat.Models;
using Kentico.Xperience.Algolia.Indexing;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using DancingGoat.Search.Services;
using DancingGoat.Search.Models;
using Kentico.Xperience.Algolia.Search;

namespace DancingGoat.Search;

public class AdvancedSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;

    public const string INDEXED_WEBSITECHANNEL_NAME = "DancingGoatPages";

    public AdvancedSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
        WebScraperHtmlSanitizer htmlSanitizer,
        WebCrawlerService webCrawler
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
        this.htmlSanitizer = htmlSanitizer;
        this.webCrawler = webCrawler;
    }

    public override IndexSettings GetAlgoliaIndexSettings() => new()
    {
        AttributesToRetrieve = new List<string>
            {
                nameof(DancingGoatSearchResultModel.Title),
                nameof(DancingGoatSearchResultModel.SortableTitle),
                nameof(DancingGoatSearchResultModel.Content)
            },
        AttributesForFaceting = new List<string>
            {
                nameof(DancingGoatSearchResultModel.ContentTypeName)
            }
    };

    public override async Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        var resultProperties = new DancingGoatSearchResultModel();

        // IIndexEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (algoliaPageItem is IndexEventWebPageItemModel indexedPage)
        {
            if (string.Equals(algoliaPageItem.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                // The implementation of GetPage<T>() is below
                var page = await GetPage<ArticlePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                resultProperties.SortableTitle = resultProperties.Title = page?.ArticleTitle ?? "";

                string rawContent = await webCrawler.CrawlWebPage(page!);
                resultProperties.Content = htmlSanitizer.SanitizeHtmlDocument(rawContent);
            }
            else if (string.Equals(algoliaPageItem.ContentTypeName, HomePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
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

                resultProperties.Title = page!.HomePageBanner.First().BannerHeaderText;
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

        var result = new List<JObject>()
        {
            AssignProperties(resultProperties)
        };

        return result;
    }

    public override async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem)
    {
        var reindexedItems = new List<IIndexEventItemModel>();

        if (string.Equals(changedItem.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var query = new ContentItemQueryBuilder()
                .ForContentType(ArticlePage.CONTENT_TYPE_NAME,
                    config =>
                        config
                            .WithLinkedItems(4)

                            // Because the changedItem is a reusable content item, we don't have a website channel name to use here
                            // so we use a hardcoded channel name.
                            //
                            // This will be resolved with an upcoming Xperience by Kentico feature
                            // https://roadmap.kentico.com/c/193-new-api-cross-content-type-querying
                            .ForWebsite(INDEXED_WEBSITECHANNEL_NAME)

                            // Retrieves all ArticlePages that link to the Article through the ArticlePage.ArticlePageArticle field
                            .Linking(nameof(ArticlePage.ArticlePageTeaser), new[] { changedItem.ItemID }))
                .InLanguage(changedItem.LanguageName);

            var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<ArticlePage>);

            foreach (var articlePage in result)
            {
                // This will be a IIndexEventItemModel passed to our MapToAlgoliaDocumentOrNull method above
                reindexedItems.Add(new IndexEventWebPageItemModel(
                    articlePage.SystemFields.WebPageItemID,
                    articlePage.SystemFields.WebPageItemGUID,
                    changedItem.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME,
                    articlePage.SystemFields.WebPageItemName,
                    articlePage.SystemFields.ContentItemIsSecured,
                    articlePage.SystemFields.ContentItemContentTypeID,
                    articlePage.SystemFields.ContentItemCommonDataContentLanguageID,
                    INDEXED_WEBSITECHANNEL_NAME,
                    articlePage.SystemFields.WebPageItemTreePath,
                    articlePage.SystemFields.WebPageItemParentID,
                    articlePage.SystemFields.WebPageItemOrder));
            }
        }

        return reindexedItems;
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

    private JObject AssignProperties<T>(T value) where T : AlgoliaSearchResultModel
    {
        var jObject = new JObject();

        foreach (var prop in value.GetType().GetProperties())
        {
            var type = prop.PropertyType;
            if (type == typeof(string))
            {
                jObject[prop.Name] = (string)prop.GetValue(value);
            }
            else if (type == typeof(int))
            {
                jObject[prop.Name] = (int)prop.GetValue(value);
            }
            else if (type == typeof(bool))
            {
                jObject[prop.Name] = (bool)prop.GetValue(value);
            }
        }

        return jObject;
    }
}
