# Create a custom index strategy

The primary functionality of this library is enabled through a custom "indexing strategy" which is entirely based on your
content model and search experience. Below we will look at the steps and features available to define this indexing process.

## Implement an index strategy type

Define a custom `DefaultAlgoliaIndexingStrategy` implementation to customize how page or content items are processed for indexing.

Your custom implemention of `DefaultAlgoliaIndexingStrategy` can use dependency injection to define services and configuration used for gathering the content to be indexed. `DefaultAlgoliaIndexingStrategy` implements `IAlgoliaIndexingStrategy` and will be [registered as a transient](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#transient) in the DI container.

## Specify a mapping process

Override the `IndexSettings GetAlgoliaIndexSettings()` method to specify saved attributes and their functionality. See Algolia documentation for this.

Override the `Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel item)` method and define a process for mapping custom fields of each content item event provided.

The method is given an `IIndexEventItemModel` which is a abstraction of any item being processed for indexing, which includes both `IndexEventWebPageItemModel` for web page items and `IndexEventReusableItemModel` for reusable content items. Every item specified in the admin ui is rebuilt. In the UI you need to specify one or more language, channel name, indexingStrategy and paths with content types. This strategy than evaluates all web page items specified in the administration.

Let's say we specified `ArticlePage` in the admin ui.
Now we implement how we want to save ArticlePage page in our strategy.

The JObject is indexed representation of the webpageitem.

You specify what fields should be indexed in the JObject by adding them to the `IndexSettings`. You later retrieve data from the JObject based on your implementation.

```csharp
public class ExampleSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    public static string SORTABLE_TITLE_FIELD_NAME = "SortableTitle";

    public override IndexSettings GetAlgoliaIndexSettings() =>
        new()
        {
            AttributesToRetrieve = new List<string>
            {
                nameof(DancingGoatSimpleSearchResultModel.Title)
            }
        };

    public override async Task<IEnumerable<JObject>> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
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
}
```

Some properties of the `IIndexEventItemModel` are added to the JObjects by default by the library and these can be found in the `AlgoliaSearchResultModel` class.

```csharp
// BaseJObjectProperties.cs

public class AlgoliaSearchResultModel
{
    // This field is defaultly only added to the document if the indexed item is a web page.
    public string Url { get; set; } = "";
    public string ContentTypeName { get; set; } = "";
    public string LanguageName { get; set; } = "";
    public Guid ItemGuid { get; set; }
    public string ObjectID { get; set; } = "";
}
```

Override the class and use the name of these properties to specify the `IndexSettings` and later use this class to retrieve these data from Indexed Object.

```csharp
public class DancingGoatSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
```

```csharp
public class ExampleSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    // ...

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

    public override async Task<IEnumerable<JObject>> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        var resultProperties = new DancingGoatSearchResultModel();

        // ...

        var result = new List<JObject>()
        {
            AssignProperties(resultProperties)
        };

        return result;
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
```

The `Url` field is a relative path by default. You can change this by adding this field manually in the `MapToAlgoliaJObjectsOrNull` method.

```csharp
public override async Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel item)
{
    //...

    var result = new List<JObject>();

    // retrieve an absolute URL
    if (item is IndexEventWebPageItemModel webpageItem &&
        string.Equals(indexedModel.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
    {
        string url = string.Empty;
        try
        {
            url = (await urlRetriever.Retrieve(
                webpageItem.WebPageItemTreePath,
                webpageItem.WebsiteChannelName,
                webpageItem.LanguageName)).AbsolutePath;
        }
        catch (Exception)
        {
            // Retrieve can throw an exception when processing a page update AlgoliaQueueItem
            // and the page was deleted before the update task has processed. In this case, upsert an
            // empty URL
        }

        var jObject = new JObject();
            jObject[nameof(AlgoliaSearchResultModel.Url)] = url;
    }

    //...
}
```

## Data retrieval during indexing

It is up to your implementation how do you want to retrieve the content or data to be indexed, however any web page item could be retrieved using a generic `GetPage<T>` method. In the example below, you specify that you want to retrieve `ArticlePage` item in the provided language on the channel using provided id and content type.

```csharp
public class ExampleSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    // Other fields defined in previous examples
    // ...

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override IndexSettings GetAlgoliaIndexSettings()
    {
        // Same as examples above
        // ...
    }

    public override async Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
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
        // Same as examples above
        // ...
    }
}
```

## Keeping indexed related content up to date

If an indexed web page item has relationships to other web page items or reusable content items, and updates to those items should trigger
a reindex of the original web page item, you can override the `Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem)` or `Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem)` methods which both return the items that should be indexed based on the incoming item being changed.

In our example an `ArticlePage` web page item has a `ArticlePageArticle` field which represents a reference to related reusable content items that contain the full article content. We include content from the reusable item in our indexed web page, so changes to the reusable item should result in the index being updated for the web page item.

All items returned from either `FindItemsToReindex` method will be passed to `Task<IEnumerable<JObject>> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)` for indexing.

```csharp
public class ExampleSearchIndexingStrategy : DefaultAlgoliaIndexingStrategy
{
    // Other fields defined in previous examples
    // ...

    public const string INDEXED_WEBSITECHANNEL_NAME = "mywebsitechannel";

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchIndexingStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override IndexSettings GetAlgoliaIndexSettings()
    {
        // Same as examples above
        // ...
    }

    public override async Task<IEnumerable<JObject>> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
    }

    public override async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem)
    {
        var reindexedItems = new List<IIndexEventItemModel>();

        if (string.Equals(indexedModel.ContentTypeName, Article.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
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
                            .Linking(nameof(ArticlePage.ArticlePageArticle), new[] { changedItem.ItemID }))
                .InLanguage(changedItem.LanguageName);

            var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<ArticlePage>);

            foreach (var articlePage in result)
            {
                // This will be a IIndexEventItemModel passed to our MapToAlgoliaJObjectsOrNull method above
                reindexable.Add(new IndexEventWebPageItemModel(
                    page.SystemFields.WebPageItemID,
                    page.SystemFields.WebPageItemGUID,
                    changedItem.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME,
                    page.SystemFields.WebPageItemName,
                    page.SystemFields.ContentItemIsSecured,
                    page.SystemFields.ContentItemContentTypeID,
                    page.SystemFields.ContentItemCommonDataContentLanguageID,
                    INDEXED_WEBSITECHANNEL_NAME,
                    page.SystemFields.WebPageItemTreePath,
                    page.SystemFields.WebPageItemParentID,
                    page.SystemFields.WebPageItemOrder));
            }
        }

        return reindexedItems;
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        // Same as examples above
        // ...
    }

    private JObject AssignProperties<T>(T value) where T : AlgoliaSearchResultModel
    {
        // Same as examples above
        // ...
    }
}
```

Note that we are not preparing the Algolia `JObject` in `FindItemsToReindex`, but instead are generating a collection of
additional items that will need reindexing based on the modification of a related `IIndexEventItemModel`.

## Indexing web page content

See [Scraping web page content](Scraping-web-page-content.md)

## DI Registration

Finally, add this library to the application services, registering your custom `DefaultAlgoliaIndexingStrategy` and Algolia

```csharp
// Program.cs

// Registers all services and uses default indexing behavior (no custom data will be indexed)
services.AddKenticoAlgolia();

// or

// Registers all services and enables custom indexing behavior
services.AddKenticoAlgolia(builder =>
    builder
        .RegisterStrategy<ExampleSearchIndexingStrategy>("ExampleStrategy")
        ,configuration);
```
