using Algolia.Search.Models.Settings;

using CMS.ContentEngine;
using CMS.Websites;

using DancingGoat.Models;
using DancingGoat.Search.Models;
using DancingGoat.Search.Services;

using Kentico.Xperience.Algolia.Indexing;
using Kentico.Xperience.Algolia.Search;

using Newtonsoft.Json.Linq;

namespace DancingGoat.Search;

public class ReusableContentItemsIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    public const string SORTABLE_TITLE_FIELD_NAME = "SortableTitle";

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;
    private readonly IWebPageUrlRetriever urlRetriever;
    private readonly WebScraperHtmlSanitizer htmlSanitizer;
    private readonly WebCrawlerService webCrawler;

    public const string FACET_DIMENSION = "ContentType";
    public const string INDEXED_WEBSITECHANNEL_NAME = "DancingGoatPages";
    public const string CRAWLER_CONTENT_FIELD_NAME = "Content";

    public ReusableContentItemsIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
        IWebPageUrlRetriever urlRetriever,
        WebScraperHtmlSanitizer htmlSanitizer,
        WebCrawlerService webCrawler
    )
    {
        this.urlRetriever = urlRetriever;
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
        if (algoliaPageItem is not IndexEventReusableItemModel indexedItem)
        {
            return null;
        }
        if (string.Equals(algoliaPageItem.ContentTypeName, Banner.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
        {
            var query = new ContentItemQueryBuilder()
            .ForContentType(HomePage.CONTENT_TYPE_NAME,
                config =>
                    config
                        .WithLinkedItems(4)
                        // Because the changedItem is a reusable content item, we don't have a website channel name to use here
                        // so we use a hardcoded channel name.
                        .ForWebsite(INDEXED_WEBSITECHANNEL_NAME)
                        // Retrieves all HomePages that link to the Banner through the HomePage.HomePageBanner field
                        .Linking(nameof(HomePage.HomePageBanner), new[] { indexedItem.ItemID }))
            .InLanguage(indexedItem.LanguageName);

            var associatedWebPageItem = (await queryExecutor.GetWebPageResult(query, webPageMapper.Map<HomePage>)).First();
            string url = string.Empty;
            try
            {
                url = (await urlRetriever.Retrieve(associatedWebPageItem.SystemFields.WebPageItemTreePath,
                    INDEXED_WEBSITECHANNEL_NAME, indexedItem.LanguageName)).RelativePath;
            }
            catch (Exception)
            {
                // Retrieve can throw an exception when processing a page update LuceneQueueItem
                // and the page was deleted before the update task has processed. In this case, return no item.
                return null;
            }

            //If the indexed item is a reusable content item, we need to set the url manually.
            resultProperties.Url = url;
            resultProperties.SortableTitle = resultProperties.Title = associatedWebPageItem!.HomePageBanner.First().BannerText;
            string rawContent = await webCrawler.CrawlWebPage(associatedWebPageItem!);
            resultProperties.Content = htmlSanitizer.SanitizeHtmlDocument(rawContent);

            //If the indexed item is a reusable content item, we need to set the url manually.
            var result = new List<JObject>()
            {
                AssignProperties(resultProperties)
            };

            return result;
        }
        else
        {
            return null;
        }
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
