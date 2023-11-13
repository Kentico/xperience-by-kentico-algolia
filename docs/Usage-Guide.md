# Usage Guide

## Introduction

A single class (created by the developers) contains the Algolia index attributes and the individual attribute configurations, which are registered during application startup. As a result, your developers can utilize Algolia's [POCO philosophy](https://www.algolia.com/doc/api-client/getting-started/install/csharp/?client=csharp#poco-types-and-jsonnet) while creating the search interface.

> :bulb: Certain code examples in this article reference and work with values and types from the Dancing Goat project. Dancing Goat is a sample project that demonstrates the content management and digital marketing features of the Xperience platform. Feel free to [install the project](https://docs.xperience.io/x/DQKQC) from a .NET template and follow along with the examples.

## Basic Setup

### Algolia Configuration

In the [Algolia dashboard](https://www.algolia.com/dashboard), open your application, navigate to **Settings → API keys** and note the _Search API key_ value.

On the **All API keys** tab, create a new "Indexing" API key which will be used for indexing and performing searches in the Xperience application. The key must have at least the following ACLs:

- search
- addObject
- deleteObject
- deleteIndex
- editSettings
- listIndexes

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

## Defining a Search Model

An Algolia index and its attributes are defined within a single class, which must inherit from [`AlgoliaSearchModel`](../src/Models/AlgoliaSearchModel.cs).

Within the class, define the attributes of the index by creating properties that match the names of the content type fields to index. The index supports fields from the `TreeNode` object and any custom fields defined using the [field editor](https://docs.xperience.io/x/RIXWCQ).

> The property names (and names used in the [SourceAttribute](#source-attribute)) are **case-insensitive**. This means that your search model can contain an "articletext" property, or an "ArticleText" property - both will work. We recommending using the `nameof()` operator to define these values.

```cs
public class SiteSearchModel : AlgoliaSearchModel
{
    public const string IndexName = "SiteIndex";

    [Searchable, Retrievable]
    public string DocumentName { get; set; }

    [MediaUrls, Retrievable, Source(new string[] { nameof(Article.ArticleTeaser), nameof(Coffee.CoffeeImage) })]
    public IEnumerable<string> Thumbnail { get; set; }

    [Searchable, Retrievable, Source(new string[] { nameof(Article.ArticleSummary), nameof(Coffee.CoffeeShortDescription) })]
    public string ShortDescription { get; set; }

    [Searchable, Source(new string[] { nameof(Article.ArticleText), nameof(Coffee.CoffeeDescription) })]
    public string Content { get; set; }

    [Facetable]
    public string CoffeeProcessing { get; set; }

    [Facetable]
    public bool CoffeeIsDecaf { get; set; }
}
```

### ASP.NET Core Setup

In `Program.cs`, register the Algolia integration using the `AddAlgolia()` extension method:

```cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddKentico();
builder.Services.AddAlgolia(builder.Configuration);
```

This method accepts a list of [`AlgoliaIndex`](/src/Models/AlgoliaIndex.cs) instances, allowing you to create and register as many indexes as needed:

```cs
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), SiteSearchModel.IndexName),
    // Additional index registrations as needed...
});
```

If you're developing your search solution in multiple environments (e.g. "DEV" and "STAGE"), we recommended that you create a unique Algolia index per environment. With this approach, the search functionality can be tested in each environment individually and changes to the index structure or content will not affect other environments. This can be implemented any way you'd like, including some custom service which transforms the index names. The simplest approach is to prepend the environment name, stored in the `appsettings.json`, to the index:

```json
"Environment": "DEV",
```

Then, reference this configuration value when defining the index name:

```cs
string environment = builder.Configuration["Environment"];
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), $"{environment}-{SiteSearchModel.IndexName}")
});
```

This environment value can be populated in the `appsettings.json` file in a build pipeline (ex: GitHub Actions or Azure DevOps).

## Content Indexing Customization

### Determining which pages to index

While the above sample code will create an Algolia index, pages in the content tree will not be indexed until one or more [`IncludedPathAttribute`](../src/Attributes/IncludedPathAttribute.cs) attributes are applied to the class. The `IncludedPathAttribute` has two properties to configure:

- **AliasPath**: The path in the content tree to index. Use a wildcard `"/%"` value to index all children of an `AliasPath`.
- **ContentTypes** (optional): The code names of the Page content types under the specified [`AliasPath`](https://docs.xperience.io/x/4obWCQ#Retrievepagecontent-Pagepathexpressions) to index. If this value is not provided, all content types are indexed.

> We recommend using [generated code files](https://docs.xperience.io/x/5IbWCQ) to reference Page content type class names.

The code sample below demonstrates using `IncludedPathAttribute` to include multiple paths and Page content types in an index:

```cs
[IncludedPath("/Articles/%", ContentTypes = new string[] { Article.CLASS_NAME })]
[IncludedPath("/Coffees/%", ContentTypes = new string[] { Coffee.CLASS_NAME })]
public class SiteSearchModel : AlgoliaSearchModel
{
    // ...
}
```

### Customizing the indexing process

In some cases, you may want to customize the values that are sent to Algolia during the indexing process. For example, the [`SiteSearchModel`](#defining-a-search-model) search model above contains the `Content` property which retrieves its value from the `ArticleText` or `CoffeeDescription` fields. However, the content of your pages may be retrieved from [linked content items](https://docs.xperience.io/xp/developers-and-admins/development/content-modeling/content-types#Contenttypes-Addoptiontolinkcontentitems) instead.

To customize the indexing process, you can override the `OnIndexingProperty()` that is defined in the search model base class `AlgoliaSearchModel`. This method is called during the indexing of a page for each property defined in your search model. You can use the function parameters such as the page being indexed, the value that would be indexed, the search model property name, and the name of the database column the value was retrieved from.

We can use the [generated code](https://docs.xperience.io/xp/developers-and-admins/development/content-retrieval/generate-code-files-for-xperience-objects) of a content type to retrieve the text from the linked content items. If we return the combined text from the linked content types, it will be stored in our "Content" field:

```cs
public override object OnIndexingProperty(TreeNode node, string propertyName, string usedColumn, object foundValue)
{
    switch (propertyName)
    {
        case nameof(Content):
            if (node.ClassName.Equals(Parent.CLASS_NAME, System.StringComparison.OrdinalIgnoreCase))
            {
            var text = new StringBuilder();
            var parentPage = node as Parent;
            foreach (var section in parentPage.Fields.Sections)
            {
                var sectionText = section.GetStringValue(nameof(Section.SectionText), String.Empty);
                text.Append(sectionText);
            }
            return text.ToString();
            }
            break;
    }

   return base.OnIndexingProperty(node, propertyName, usedColumn, foundValue);
}
```

### Algolia attributes

The integration package includes five attributes which can be applied to individual Algolia index attributes to further configure the Algolia index:

- [`Searchable`](#searchable-attribute)
- [`Facetable`](#facetable-attribute)
- [`Retrievable`](#retrievable-attribute)
- [`Source`](#source-attribute)
- [`MediaUrls`](#mediaurls-attribute)

#### Searchable attribute

This attribute indicates that an Algolia index attribute is [searchable](https://www.algolia.com/doc/api-reference/api-parameters/searchableAttributes/#how-to-use). You can define optional attribute properties to adjust the performance of your searchable index attributes:

- **Order** (optional): Index attributes with lower `Order` will be given priority when searching for text. Index attributes without `Order` set will be added to the end of the list (making them lowest priority), while attributes with the same `Order` will be added with the same priority and are automatically `Unordered`.
- **Unordered** (optional): By default, matches at the beginning of a text are more relevant than matches at the end of the text. If set to `true`, the position of the matched text in the attribute content is irrelevant.

```cs
[Searchable]
public string DocumentName { get; set; }

[Searchable(Order = 0)] // Highest priority
public string DocumentName { get; set; }

[Searchable(Unordered = true)]
public string DocumentName { get; set; }
```

#### Facetable attribute

This attribute indicates that an Algolia index attribute is a [facet or filter](https://www.algolia.com/doc/api-reference/api-parameters/attributesForFaceting/#how-to-use). By creating facets, your developers are able to create a [faceted search](https://www.algolia.com/doc/guides/managing-results/refine-results/faceting/) interface on the front-end application. Optional attribute properties can be defined to change the functionality of your faceted index attributes:

- **FilterOnly** (optional): Defines the attribute as a filter and not a facet. If you do not need facets, defining an attribute as a filter reduces the size of the index and improves the speed of the search. Defaults to `false`.

- **Searchable** (optional): Allows developers to search for values within a facet, e.g. via the [`SearchForFacetValues()`](https://www.algolia.com/doc/api-reference/api-methods/search-for-facet-values/) method. Defaults to `false`.

- **UseAndCondition** (optional): When using the sample code in this repository and the `AlgoliaFacetFilterViewModel` class, facet conditions of the same properties are joined by "OR" by default. For example, `(CoffeProcessing:washed OR CoffeeProcessing:natural)`. You may set this property to `true` to join them by "AND" instead. Defaults to `false`.

> A property cannot be both `FilterOnly` and `Searchable`, otherwise an exception will be thrown.

```cs
[Facetable]
public string CoffeeProcessing { get; set; }

[Facetable(FilterOnly = true)]
public string CoffeeProcessing { get; set; }

[Facetable(Searchable = true)]
public string CoffeeProcessing { get; set; }
```

#### Retrievable attribute

This attribute determines which Algolia index attributes to [retrieve when searching](https://www.algolia.com/doc/api-reference/api-parameters/attributesToRetrieve/#how-to-use). Reducing the number of retrieved index attributes helps improve the speed of your searches without impacting the search functionality.

```cs
[Searchable, Retrievable] // Used during search and retrieval
public string DocumentName { get; set; }

[Searchable] // Used when searching but not during retrieval
public string ArticleText { get; set; }
```

#### Source attribute

You can use the `Source` attribute to specify which content type fields are stored in a given Algolia index attribute.

By default, the value stored in the index attribute is retrieved from the content type field that matches the name of the declared property in the search model class.

In certain cases however, you may wish to include content from multiple fields under a single index attribute. For example, if your project doesn't use uniform field naming conventions across content types, or you need to index multiple fields from a single content type under one index attribute.

```cs
// Ensures the 'Content' index attribute contains values from both the 'ArticleText' and 'CoffeeDescription' fields
[Searchable, Source(new string[] { nameof(Article.ArticleText), nameof(Coffee.CoffeeDescription) })]
public string Content { get; set; }
```

Fields specified in the `Source` attribute are parsed in the order they appear, until a non-empty string and non-null value is found, which is then indexed. When referencing content type fields, use the `nameof()` expression to avoid typos.

### MediaUrls attribute

This attribute is intended for fields that use the Xperience ["Media files" data type](https://docs.xperience.io/x/RoXWCQ). When the page is indexed, the files are converted into a list of live-site URLs.

```cs
[MediaUrls, Retrievable]
public string ArticleTeaser { get; set; }

[MediaUrls, Retrievable, Source(new string[] { nameof(Article.ArticleTeaser), nameof(Coffee.CoffeeImage) })]
public IEnumerable<string> Thumbnail { get; set; }
```

### Splitting large content

Due to [limitations](https://support.algolia.com/hc/en-us/articles/4406981897617-Is-there-a-size-limit-for-my-index-records-/) on the size of Algolia records, we recommend splitting large content into smaller fragments. When enabled, this operation is performed automatically during indexing by [`IAlgoliaObjectGenerator.SplitData()`](../src/Services/IAlgoliaObjectGenerator.cs), but data splitting is _not_ enabled by default.

To enable data splitting for an Algolia index, add the `DistinctOptions` parameter during registration:

```cs
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), SiteSearchModel.IndexName, new DistinctOptions(nameof(SiteSearchModel.DocumentName), 1))
});
```

The `DistinctOptions` constructor accepts two parameters:

- **distinctAttribute**: Corresponds with the [Algolia `attributeForDistinct` parameter](https://www.algolia.com/doc/api-reference/api-parameters/attributeForDistinct). This is a property of the search model whose value will remain constant for all fragments, and is used to identify fragments during de-duplication. Fragments of a search result are "grouped" together according to this attribute's value, then a certain number of fragments per-group are returned, depending on the `distinctLevel` setting. In most cases, this will be a property like `DocumentName` or `NodeAliasPath`.
- **distinctLevel**: Corresponds with the [Algolia `distinct` parameter](https://www.algolia.com/doc/api-reference/api-parameters/distinct). A value of zero disables de-duplication and grouping, while positive values determine how many fragments will be returned by a search. This is generally set to `1` so that only one fragment is returned from each grouping.

To implement data splitting, create and register a custom implementation of `IAlgoliaObjectGenerator`. It's **very important** to set the "objectID" of each fragment, as seen in the example below. The IDs can be any arbitrary string, but setting this ensures that the fragments are updated and deleted properly when the page is modified. We recommend developing a consistent naming strategy like in the example below, where an index number is appended to the original ID. The IDs **_must not_** be random! Calling `SplitData()` on the same node multiple times should always generate the same fragments and IDs.

In the following example, we have large articles on our website which can be split into smaller fragments by splitting text on the `<p>` tag. Note that each fragment still contains all of the original data- only the "Content" property is modified.

```cs
[assembly: RegisterImplementation(typeof(IAlgoliaObjectGenerator), typeof(CustomAlgoliaObjectGenerator))]

namespace DancingGoat.Search;

public class CustomAlgoliaObjectGenerator : IAlgoliaObjectGenerator
{
    private readonly IAlgoliaObjectGenerator defaultImplementation;

    public CustomAlgoliaObjectGenerator(IAlgoliaObjectGenerator defaultImplementation)
    {
        this.defaultImplementation = defaultImplementation;
    }

    public JObject GetTreeNodeData(AlgoliaQueueItem queueItem)
    {
        return defaultImplementation.GetTreeNodeData(queueItem);
    }

    public IEnumerable<JObject> SplitData(JObject originalData, AlgoliaIndex algoliaIndex)
    {
        if (algoliaIndex.Type == typeof(SiteSearchModel))
        {
            return SplitParagraphs(originalData, nameof(SiteSearchModel.Content));
        }

        return new JObject[] { originalData };
    }

    private IEnumerable<JObject> SplitParagraphs(JObject originalData, string propertyToSplit)
    {
        var originalId = originalData.Value<string>("objectID");
        var content = originalData.Value<string>(propertyToSplit);
        if (string.IsNullOrEmpty(content))
        {
            return new JObject[] { originalData };
        }

        List<string> paragraphs = new List<string>();
        var matches = Regex.Match(content, @"<p>\s*(.+?)\s*</p>");
        while (matches.Success)
        {
            paragraphs.Add(matches.Value);
            matches = matches.NextMatch();
        }

        return paragraphs.Select((p, index) => {
            var data = (JObject)originalData.DeepClone();
            data["objectID"] = $"{originalId}-{index}";
            data[propertyToSplit] = p;
            return data;
        });
    }
}
```

## Executing search requests

### Basic Search

You can use Algolia's [.NET API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/csharp/?client=csharp), [JavaScript API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/javascript/?client=javascript), or [InstantSearch.js](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) to implement a search interface on your live site.

The following example will help you with creating a search interface for .NET. In your ASP.NET Core code, you can access a `SearchIndex` object by injecting the `IAlgoliaIndexService` interface and calling the `InitializeIndex()` method using your index's code name. Then, construct a `Query` to search the Algolia index. Algolia's pagination is zero-based, so in the Dancing Goat sample project we subtract 1 from the current page number:

```cs
private readonly IAlgoliaIndexService _indexService;

public SearchController(IAlgoliaIndexService indexService)
{
    _indexService = indexService;
}

public async Task<ActionResult> Search(string searchText, CancellationToken cancellationToken, int page = DEFAULT_PAGE_NUMBER)
{
    page = Math.Max(page, DEFAULT_PAGE_NUMBER);

    var searchIndex = await _indexService.InitializeIndex(SiteSearchModel.IndexName, cancellationToken);
    var query = new Query(searchText)
    {
        Page = page - 1,
        HitsPerPage = PAGE_SIZE
    };

    try
    {
        var results = await searchIndex.SearchAsync<SiteSearchModel>(query, ct: cancellationToken);
        //...
    }
    catch (Exception e)
    {
        //...
    }
}
```

The `Hits` object of the [search response](https://www.algolia.com/doc/api-reference/api-methods/search/?client=csharp#response) is a list of strongly typed objects defined by your search model (`SiteSearchModel` in the example above). Other helpful properties of the results object are `NbPages` and `NbHits`.

The properties of each hit are populated from the Algolia index, but be sure not to omit `null` checks when working with the results. For example, a property that does _not_ have the [`Retrievable`](#retrievable-attribute) attribute is not returned and custom content type fields are only present for results of that type. That is, a property named "ArticleText" will be `null` for the coffee pages in Dancing Goat. You can reference the [`SiteSearchModel.ClassName`](../src/Models/AlgoliaSearchModel.cs) property present on all indexes to check the type of the returned hit.

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

After [installing](../README.md#package-installation) the NuGet package in your Xperience by Kentico project, a new _Search_ application becomes available in the **Development** application group. The Search application displays a table of all registered Algolia indexes with information about the number of records, build time, and last update:

<a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/main-menu.png">
  <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/main-menu.png" width="600" alt="Administration Algolia indexes list">
</a>

Use the **Rebuild** action on the right side of the table to re-index the pages of the Algolia index. This completely removes the existing records and replaces them with the most up-to-date data. Rebuilding indexes is especially useful after enabling the [data splitting](#splitting-large-content) feature. Selecting an index form the list displays a page detailing the indexed paths and properties of the corresponding Algolia index:

<a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/indexed-content-menu.png">
  <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/indexed-content-menu.png" width="600" alt="Administration indexed content details">
</a>

The **Indexed properties** table lists each property defined in the search model and the [attributes](#algolia-attributes) of that property.

The **Indexed paths** table lists the search model's [`IncludedPathAttribute`s](#determining-which-pages-to-index), including the paths and content types included within each index attribute.
Selecting an indexed path displays each content type included in the indexed path:

<a href="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/path-detail-menu.png">
  <img src="https://raw.githubusercontent.com/Kentico/xperience-by-kentico-algolia/main/images/path-detail-menu.png" width="600" alt="Administration indexed content path details">
</a>

## Advanced Topics

There are more topics covered by additional pages in this guide:

- [Building a Search UI](./Build-Search-UI.md)
- [Search Personalization](./Search-Personalization.md)
- [InstantSearch.js](./InstantSearch-js.md)
- [Algolia Crawler](./Algolia-Crawler.md)

## Additional Resources
