# Usage Guide

## Introduction

A single class (created by the developers) contains the Algolia index setup and methods to match Kentico page and content items to indexed recors. As a result, your developers can utilize Algolia's [POCO philosophy](https://www.algolia.com/doc/api-client/getting-started/install/csharp/?client=csharp#poco-types-and-jsonnet) while creating the search interface.

## Basic Setup

### Algolia Configuration

In the [Algolia dashboard](https://www.algolia.com/dashboard), open your application, navigate to **Settings → API keys** and note the _Search API key_ value.

On the **All API keys** tab, create a new "Indexing" API key which will be used for indexing and performing searches in the Xperience application. The key must have at least the following ACLs:

### ASP.NET Core Configuration

In the Xperience project's `appsettings.json`, add the following section with your API key values:

> :warning: Do not use the Admin API key! Use the custom "Indexing" API key you just created. :warning:

```json
"xperience.algolia": {
    "applicationId": "<your application ID>",
    "apiKey": "<your Indexing API key>",
    "searchKey": "<your Search API key>"
}
```

### Create a custom Indexing Strategy

Define a custom `DefaultAlgoliaIndexingStrategy` implementation to customize how page or content items are processed for the index.

First You should define a custom class inheriting the `AlgoliaSearchResultModel` which will be used to retrieve the indexed data from the Algolia index.
> The property names are **case-insensitive**. This means that your search result model can contain an "articletext" property, or an "ArticleText" property - both will work. These properties are mapped to the attributes which you create on the Algolia index.
>  We recommend using `nameof()` of these attributes to specify the settings of the Algolia Index.

```csharp
public class ExampleSearchResultModel : AlgoliaSearchResultModel
{
    public string ContentType { get; set; }
    public string SortableTitle { get; set; }
    public string Title { get; set; }
    public string CrawlerContent { get; set; }
}
```

The `GetAlgoliaIndexSettings` method is used to specify Algolia Settings.
You should specify your Searchable, Retrievable and Facetable attributes here. Remember you can use the `nameof()` of the attributes defined in the `ExampleSearchResultModel` to specify these attributes.
```csharp
public class GlobalAlgoliaIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    public override IndexSettings GetAlgoliaIndexSettings()
    {
        return new IndexSettings()
        {
            SearchableAttributes = new List<string> { $"{nameof(ExampleSearchResultModel.ContentType)},{nameof(ExampleSearchResultModel.SortableTitle)},{nameof(ExampleSearchResultModel.Title)},{nameof(ExampleSearchResultModel.CrawlerContent)}" },
            AttributesToRetrieve = new List<string>
            {
                nameof(ExampleSearchResultModel.ContentType),
                nameof(ExampleSearchResultModel.SortableTitle),
                nameof(ExampleSearchResultModel.Title)
            },
            AttributesForFaceting = new List<string> { nameof(ExampleSearchResultModel.ContentType) }
        };
    }

    //...
}
```

`MapToAlgoliaJObjecstOrNull` method is given an `IndexedItemModel` which is a unique representation of any item used on a web page. Every item of a type specified in the admin ui is rebuilt. In the UI you need to specify one or more languages, channel name, indexingStrategy and paths with content types. This strategy than evaluates all web page items of a type specified in the administration. 

Let's say we specified `ArticlePage` in the admin ui.
Now we implement how we want to save ArticlePage document in our strategy.

The document is indexed representation of the webpageitem.

You should create JObject which is an object used to represent indexed data. 

```csharp
public class GlobalAlgoliaIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    public override IndexSettings GetAlgoliaIndexSettings()
    {
        return new IndexSettings()
        {
            SearchableAttributes = new List<string> { $"{nameof(ExampleSearchResultModel.ContentType)},{nameof(ExampleSearchResultModel.SortableTitle)},{nameof(ExampleSearchResultModel.Title)},{nameof(ExampleSearchResultModel.CrawlerContent)}" },
            AttributesToRetrieve = new List<string>
            {
                nameof(ExampleSearchResultModel.ContentType),
                nameof(ExampleSearchResultModel.SortableTitle),
                nameof(ExampleSearchResultModel.Title)
            },
            AttributesForFaceting = new List<string> { nameof(ExampleSearchResultModel.ContentType) }
        };
    }

    public override async Task<IEnumerable<JObject>?> MapToAlgoliaJObjecstOrNull(IndexedItemModel algoliaPageItem)
    {
        var result = new JObject();

        var exampleResultModel = new ExampleSearchResultModel();

        if (algoliaPageItem.WebPageItemTreePath.Contains("Search"))
        {
            return null;
        }

        if (algoliaPageItem.ClassName == ArticlePage.CONTENT_TYPE_NAME)
        {
            var page = await GetPage<ArticlePage>(algoliaPageItem.WebPageItemGuid, algoliaPageItem.ChannelName, algoliaPageItem.LanguageCode, ArticlePage.CONTENT_TYPE_NAME);
            exampleResultModel.ContentType = "news";

            if (page != default)
            {
                var article = page.ArticlePageArticle.FirstOrDefault();

                if (article == null)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            if (page != default)
            {
                var article = page.ArticlePageArticle.FirstOrDefault();

                exampleResultModel.SortableTitle = exampleResultModel.Title = article?.ArticleTitle ?? "";
            }
        }

        exampleResultModel.CrawlerContent = await GetPageContent(algoliaPageItem);

        result[nameof(ExampleSearchResultModel.ContentType)] = exampleResultModel.ContentType;
        result[nameof(ExampleSearchResultModel.SortableTitle)] = exampleResultModel.SortableTitle;
        result[nameof(ExampleSearchResultModel.Title)] = exampleResultModel.Title;
        result[nameof(ExampleSearchResultModel.CrawlerContent)] = exampleResultModel.CrawlerContent;

        return new List<JObject>() { result };
    }
```

In case you do not want to index any page on the `IndexedItemModel` you should return null. Indexed data associated with this IndexedItemModel in the previous iterations are also deleted when you return null. You can use this to implement splitting indexed data.

### Splitting large content

Due to [limitations](https://support.algolia.com/hc/en-us/articles/4406981897617-Is-there-a-size-limit-for-my-index-records-/) on the size of Algolia records, you can split large content into smaller fragments. You need to implement how are the data split and how are they later used.

### ASP.NET Core Setup

Add this library to the application services, registering your custom `DefaultAlgoliaIndexingStrategy` and Algolia services as follows
  ```csharp
  // Program.cs
    services.AddAlgolia(configuration);
    services.RegisterStrategy<GlobalAlgoliaIndexingStrategy>("DefaulSrategy");
   ```

## Content Indexing Customization

Open the Admin UI and find Search Module. You should see a listing of all registered algolia indices.
You can create, delete and edit indices here.
To create an index you need to specify it's name, indexed languages and name of the channel. You can configure and use these accross multiple sites. This is handy because you can write code for a widget only once, but you can configure this accross sites in various languages. You can use same widget specifying a content type which you use for a q and a site. There you can implement q and a search. Imagination is your only limit.

## Executing search requests

### Basic Search

You can use Algolia's [.NET API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/csharp/?client=csharp), [JavaScript API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/javascript/?client=javascript), or [InstantSearch.js](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) to implement a search interface on your live site.

The following example will help you with creating a search interface for .NET. In your ASP.NET Core code, you can access a `SearchIndex` object by injecting the `IAlgoliaIndexService` interface and calling the `InitializeIndex()` method using your index's code name. Then, construct a `Query` to search the Algolia index. Algolia's pagination is zero-based.

```cs
public class SearchController
{
    private readonly IAlgoliaIndexService _indexService;

    public SearchController(IAlgoliaIndexService indexService)
    {
        _indexService = indexService;
    }

    [Route("searchAlgolia")]
    public async Task<ActionResult> Search(string searchText, int page, string indexName)
    {
        page = Math.Max(page, 1);

        var searchIndex = await _indexService.InitializeIndex(indexName, default);
        var query = new Query(searchText)
        {
            Page = page - 1,
            HitsPerPage = 20
        };

        try
        {
            var results = await searchIndex.SearchAsync<ExampleSearchResultModel>(query);
        }
        catch (Exception e)
        {
            //...
        }

        //...
    }
}
```

The `Hits` object of the [search response](https://www.algolia.com/doc/api-reference/api-methods/search/?client=csharp#response) is a list of strongly typed objects defined by your search model (`AlgoliaSearchResultModel` in the example above). Other helpful properties of the results object are `NbPages` and `NbHits`.

The properties of each hit are populated from the Algolia index, but be sure not to omit `null` checks when working with the results. You can reference the [`AlgoliaSearchResultModel.ClassName`](/src/Models/AlgoliaSearchResultModel.cs) property present on all indexes to check the type of the returned hit.

Once the search is performed, pass the `Hits` and paging information to your view:

```cs
return View(new SearchResultsModel()
{
    Items = results.Hits,
    Query = searchText,
    CurrentPage = page,
    NumberOfPages = results.NbPages
});
```

## Xperience Administration Algolia application

Use the **Rebuild** action on the right side of the table to re-index the pages of the Algolia index. This completely removes the existing records and replaces them with the most up-to-date data. Rebuilding indexes is especially useful after implementing the [data splitting](#splitting-large-content) feature. Selecting an index form the list displays a page detailing the indexed paths and properties of the corresponding Algolia index:

## Advanced Topics

It is up to your implementation how do you want to retrieve information about the page, however article page or any webpageitem could be retrieved using `GetPage<T>` method. Where you specify that you want to retrieve `ArticlePage` item in the provided language on the channel using provided id and content type.

```csharp
public class ExampleSearchIndexingStrategy : DefaultLuceneIndexingStrategy
{
    public string FacetDimension { get; set; } = "ContentType";
    public static string SORTABLE_TITLE_FIELD_NAME = "SortableTitle";

    private async Task<T> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName) where T : IWebPageFieldsSource, new()
    {
        var mapper = Service.Resolve<IWebPageQueryResultMapper>();
        var executor = Service.Resolve<IContentQueryExecutor>();
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config =>
                    config
                        .WithLinkedItems(4)
                        .ForWebsite(channelName, includeUrlPath: true)
                        .Where(where => where.WhereEquals(nameof(IWebPageContentQueryDataContainer.WebPageItemGUID), id))
                        .TopN(1))
            .InLanguage(languageName);
        var result = await executor.GetWebPageResult(query, container => mapper.Map<T>(container), null,
        cancellationToken: default);

        return result.FirstOrDefault();
    }

    public override FacetsConfig FacetsConfigFactory()
    {
        var facetConfig = new FacetsConfig();

        facetConfig.SetMultiValued(FacetDimension, true);

        return facetConfig;
    }

    public override async Task<Document?> MapToLuceneDocumentOrNull(IndexedItemModel indexedModel)
    {
        var document = new Document();

        string sortableTitle = "";
        string title = "";
        string contentType = "";
        
        if (indexedModel.ClassName == ArticlePage.CONTENT_TYPE_NAME)
        {
            var page = await GetPage<ArticlePage>(indexedModel.WebPageItemGuid, indexedModel.ChannelName, indexedModel.LanguageCode, ArticlePage.CONTENT_TYPE_NAME);
            contentType = "news";

            if (page != default)
            {
                var article = page.ArticlePageArticle.FirstOrDefault();

                if (article == null)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            if (page != default)
            {
                var article = page.ArticlePageArticle.FirstOrDefault();

                sortableTitle = title = article?.ArticleTitle ?? "";
            }
        }

        document.Add(new FacetField(FacetDimension, contentType));

        document.Add(new TextField(nameof(GlobalSearchResultModel.Title), title, Field.Store.YES));
        document.Add(new StringField(SORTABLE_TITLE_FIELD_NAME, sortableTitle, Field.Store.YES));
        document.Add(new TextField(nameof(GlobalSearchResultModel.ContentType), contentType, Field.Store.YES));

        return document;
    }
}
```


You can also Extend this to index content of the page. This implementation is up to you, however we provide a general example which can be used in any app: 

Create a `WebCrawlerService` your baseUrl needs to mathc your site baseUrl. We retrieve this url from the appSettings.json in the  

```csharp
tring baseUrl = ValidationHelper.GetString(Service.Resolve<IAppSettingsService>()["WebCrawlerBaseUrl"], "");
```

```csharp
public class WebCrawlerService
{
    private readonly HttpClient httpClient;
    private readonly IEventLogService eventLogService;
    private readonly IWebPageUrlRetriever webPageUrlRetriever;

    public WebCrawlerService(HttpClient httpClient,
        IEventLogService eventLogService,
        IWebPageUrlRetriever webPageUrlRetriever)
    {
        this.httpClient = httpClient;
        this.httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "SearchCrawler");
        string baseUrl = ValidationHelper.GetString(Service.Resolve<IAppSettingsService>()["WebCrawlerBaseUrl"], "");
        this.httpClient.BaseAddress = new Uri(baseUrl);
        this.eventLogService = eventLogService;
        this.webPageUrlRetriever = webPageUrlRetriever;
    }

    public async Task<string> CrawlNode(IndexedItemModel itemModel)
    {
        try
        {
            //TODO MilaHlavac: improve url parts concatenation for aplications hosted on non root path
            var url = (await webPageUrlRetriever.Retrieve(itemModel.WebPageItemGuid, itemModel.LanguageCode)).RelativePath.TrimStart('~').TrimStart('/');
            return await CrawlPage(url);
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(WebCrawlerService), nameof(CrawlNode), ex, $"WebPageItemTreePath: {itemModel.WebPageItemTreePath}");
        }
        return "";
    }

    public async Task<string> CrawlPage(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            eventLogService.LogException(nameof(WebCrawlerService), nameof(CrawlPage), ex, $"Url: {url}");
        }
        return "";
    }
}
```



Create a sanitizer Service
``` csharp

public class WebScraperHtmlSanitizer
{
    public virtual string SanitizeHtmlFragment(string htmlContent)
    {

        var parser = new HtmlParser();
        // null is relevant parameter
        var nodes = parser.ParseFragment(htmlContent, null);

        // Removes script tags
        foreach (var element in nodes.QuerySelectorAll("script"))
        {
            element.Remove();
        }

        // Removes script tags
        foreach (var element in nodes.QuerySelectorAll("style"))
        {
            element.Remove();
        }

        // Removes elements marked with the default Xperience exclusion attribute
        foreach (var element in nodes.QuerySelectorAll($"*[{"data-ktc-search-exclude"}]"))
        {
            element.Remove();
        }

        // Gets the text content of the body element
        string textContent = string.Join(" ", nodes.Select(n => n.TextContent));

        // Normalizes and trims whitespace characters
        textContent = HTMLHelper.RegexHtmlToTextWhiteSpace.Replace(textContent, " ");
        textContent = textContent.Trim();

        return textContent;
    }

    public virtual string SanitizeHtmlDocument(string htmlContent)
    {
        if (!string.IsNullOrWhiteSpace(htmlContent))
        {
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(htmlContent);
            var body = doc.Body;
            if (body != null)
            {

                // Removes script tags
                foreach (var element in body.QuerySelectorAll("script"))
                {
                    element.Remove();
                }

                // Removes script tags
                foreach (var element in body.QuerySelectorAll("style"))
                {
                    element.Remove();
                }

                // Removes elements marked with the default Xperience exclusion attribute
                foreach (var element in body.QuerySelectorAll($"*[{"data-ktc-search-exclude"}]"))
                {
                    element.Remove();
                }

                // Removes header
                foreach (var element in body.QuerySelectorAll("header"))
                {
                    element.Remove();
                }

                // Removes breadcrumbs
                foreach (var element in body.QuerySelectorAll(".breadcrumb"))
                {
                    element.Remove();
                }

                // Removes footer
                foreach (var element in body.QuerySelectorAll("footer"))
                {
                    element.Remove();
                }

                // Gets the text content of the body element
                string textContent = body.TextContent;

                // Normalizes and trims whitespace characters
                textContent = HTMLHelper.RegexHtmlToTextWhiteSpace.Replace(textContent, " ");
                textContent = textContent.Trim();

                var title = doc.Head.QuerySelector("title")?.TextContent;
                var description = doc.Head.QuerySelector("meta[name='description']")?.GetAttribute("content");

                return string.Join(" ",
                    new string[] { title, description, textContent }.Where(i => !string.IsNullOrWhiteSpace(i))
                    );
            }
        }

        return string.Empty;
    }
}

```

Register these services in the startup and retrieve them in your strategy:

``` csharp
  services.AddSingleton<WebScraperHtmlSanitizer>();
  services.AddHttpClient<WebCrawlerService>();
```

``` csharp
private async Task<string> GetPageContent(IndexedItemModel indexedModel)
{
    var htmlSanitizer = Service.Resolve<WebScraperHtmlSanitizer>();
    var webCrawler = Service.Resolve<WebCrawlerService>();

    string content = await webCrawler.CrawlNode(indexedModel);
    return htmlSanitizer.SanitizeHtmlDocument(content);
}
```