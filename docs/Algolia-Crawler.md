# Algolia Crawler

The Xperience by Kentico Algolia integration provides basic support for [Algolia crawlers](https://www.algolia.com/doc/tools/crawler/getting-started/overview/). Crawlers are created and configured within [Algolia's Crawler Admin](https://crawler.algolia.com/admin), and this integration ensures that updated, archived, and deleted pages in Xperience are properly updated within the crawler.

## ASP.NET Core Setup

After you have created a crawler in Algolia, you can register the crawler in Xperience:

1. Locate the User ID and API Key in the [Algolia Crawler Admin](https://www.algolia.com/doc/tools/crawler/apis/crawler-rest-api/#authentication) and add the values to your `appsettings.json`:

```json
"xperience.algolia": {
    "crawlerUserId": "<Crawler User ID>",
    "crawlerApiKey": "<Crawler API Key>"
}
```

> Even if you are only using Algolia crawlers, you still need to include the application settings mentioned in the [Quick Start](../README.md#quick-start)

Locate the ID of the crawlers you wish to register in Xperience. The ID of each crawler can be found in the URL while navigating the Algolia Crawler Admin, or in the Algolia **Settings** menu.

In `Program.cs`, edit (or add) the `AddAlgolia` method to include one or more crawler IDs:

```cs
builder.Services.AddAlgolia(builder.Configuration, crawlers: new string[]
{
    "<Crawler ID>"
});
```

Your Xperience by Kentico application will now request re-crawling of all published pages in the content tree, and will delete records from the crawler when a page is deleted or archived.

## Algolia Setup

As the data indexed by your crawler is managed entirely by Algolia, you are welcome to configure the crawler however you'd like using [Algolia's Editor](https://www.algolia.com/doc/tools/crawler/getting-started/crawler-configuration/#how-do-you-access-a-crawler-configuration). However, the "objectID" of your records **must** be the URL of your pages! This is the default configuration, so you only need to ensure that it isn't changed. Below is a sample `actions` section of the configuration used in the Dancing Goat sample site:

```js
actions: [{
    indexName: "Dancing Goat",
    pathsToMatch: [
        "https://mysite.com/coffees/**",
        "https://mysite.com/articles/**",
    ],
    recordExtractor: ({ url, $, contentLength, fileType }) => {
        return [{
            objectID: url.href, // Do not change this!
            path: url.pathname.split("/")[1],
            fileType,
            title: $("head > title").text(),
            keywords: $("meta[name=keywords]").attr("content"),
            description: $("meta[name=description]").attr("content"),
            image: $('meta[property="og:image"]').attr("content"),
            content: $("p").text(),
        }];
    },
}],
```

## Searching with a Crawler

As your crawler can contain any number of dynamic fields in its configuration, this integration doesn't contain a strongly-typed model for crawlers. We encourage your developers to create their own model for each crawler using the example configuration above, the model could look like this:

```cs
public class CrawlerHitModel
{
    public string ObjectId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string Description { get; set; }

    public string Image { get; set; }
}
```

To perform a search against the crawler and return the `CrawlerHitModel` results, you must obtain the full index name from the crawler's configuration. Because the crawler's configuration contains a name and optional prefix that is added to the underlying index name, use `IAlgoliaClient.GetCrawler()` to retrieve the configuration, then use `IAlgoliaIndexService.InitializeCrawler()` to retrieve the search index.

In the below example we've only registered a single crawler, so we can use `FirstOrDefault()` to get the crawler ID. In cases where there are multiple crawlers registered, the developers need to create a mapping to identify which crawler is used in a particular search. We are also using the `path` and `fileType` attributes to only return pages under the `/coffees` path:

```cs
public class SearchController : Controller
{
    private readonly IAlgoliaClient algoliaClient;
    private readonly IAlgoliaIndexService algoliaIndexService;

    public SearchController(
        IAlgoliaClient algoliaClient,
        IAlgoliaIndexService algoliaIndexService)
    {
        this.algoliaClient = algoliaClient;
        this.algoliaIndexService = algoliaIndexService;
    }

    public async Task<IActionResult> Search([FromQuery] string searchText, CancellationToken cancellationToken)
    {
        // Get crawler
        var crawlerId = IndexStore.Instance.GetAllCrawlers().FirstOrDefault();
        var crawler = await algoliaClient.GetCrawler(crawlerId, cancellationToken);

        // Search
        var searchIndex = algoliaIndexService.InitializeCrawler(crawler);
        var query = new Query(searchText) {
            Filters = "path:coffees AND fileType:html"
        };
        var result = await searchIndex.SearchAsync<CrawlerHitModel>(query, ct: cancellationToken);

        return View(result.Hits);
    }
}
```

## Limitations

The [endpoint](https://www.algolia.com/doc/rest-api/crawler/#crawl-specific-urls) used to request re-crawling of updated Xperience pages has a limitation of 200 requests per day. By default, the process which requests re-crawling of pages runs every 10 minutes (114 times per day) and the limitation shouldn't be reached when a _single_ crawler is registered. However, if you have registered multiple crawlers, you will need to extend the interval by setting the `crawlerInterval` setting in `appsettings.json`:

```json
"xperience.algolia": {
    "crawlerInterval": 1200000 // 20 minutes
}
```
