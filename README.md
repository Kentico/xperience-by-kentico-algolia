[![Nuget](https://img.shields.io/nuget/v/Kentico.Xperience.Algolia)](https://www.nuget.org/packages/Kentico.Xperience.Algolia)
[![Kentico.Xperience.WebApp 22.3.0](https://img.shields.io/badge/Kentico.Xperience.WebApp-v22.3.0-orange)](https://www.nuget.org/packages/Kentico.Xperience.WebApp#versions-body-tab)
[![Algolia.Search 6.13.0](https://img.shields.io/badge/Algolia.Search-v6.13.0-blue)](https://www.nuget.org/packages/Algolia.Search#versions-body-tab)

# Xperience by Kentico Algolia Search Integration

This integration enables you to create [Algolia](https://www.algolia.com/) search indexes to index content of pages ([content types](https://docs.xperience.io/x/gYHWCQ) with the 'Page' feature enabled) from the Xperience content tree using a code-first approach. To provide a search interface for the indexed content, developers can use the [.NET API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/csharp/?client=csharp), [JavaScript API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/javascript/?client=javascript), or the [InstantSearch.js](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) library.

A single class (created by the developers) contains the Algolia index attributes and the individual attribute configurations, which are registered during application startup. As a result, your developers can utilize Algolia's [POCO philosophy](https://www.algolia.com/doc/api-client/getting-started/install/csharp/?client=csharp#poco-types-and-jsonnet) while creating the search interface.

> :bulb: Certain code examples in this article reference and work with values and types from the Dancing Goat project. Dancing Goat is a sample project that demonstrates the content management and digital marketing features of the Xperience platform. Feel free to [install the project](https://docs.xperience.io/x/DQKQC) from a .NET template and follow along with the examples.

## :rocket: Installation

1. Install the [Kentico.Xperience.Algolia](https://www.nuget.org/packages/Kentico.Xperience.Algolia) NuGet package into your project.
2. On the [Algolia dashboard](https://www.algolia.com/dashboard), open your application, navigate to __Settings ??? API keys__ and note the _Search API key_ value.
3. On the __All API keys__ tab, create a new "Indexing" API key which will be used for indexing and performing searches in the Xperience application. The key must have at least the following ACLs:
  - search
  - addObject
  - deleteObject
  - deleteIndex
  - editSettings
  - listIndexes
4. In the Xperience project's `appsettings.json`, add the following section with your API key values:

```json
"xperience.algolia": {
    "applicationId": "<your application ID>",
    "apiKey": "<your Indexing API key>",
    "searchKey": "<your Search API key>"
}
```
> :warning: Do not use the Admin API key! Use the custom API key you created in step #3.

5. In `Program.cs`, register the Algolia integration:

```cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

...
builder.Services.AddKentico();
builder.Services.AddAlgolia(builder.Configuration);
```

## Limitations

Note that Algolia has [limitations](https://support.algolia.com/hc/en-us/articles/4406981897617-Is-there-a-size-limit-for-my-index-records-/) on the size of your records. If you are indexing content that may contain large amounts of data, we recommend splitting your records into smaller "fragments." Follow the instructions in the [Splitting large content](#scissors-splitting-large-content) section.

## :gear: Creating and registering an Algolia index

An Algolia index and its attributes are defined within a single class, which must inherit from [`AlgoliaSearchModel`](/src/Models/AlgoliaSearchModel.cs). 

Within the class, define the attributes of the index by creating properties that match the names of the content type fields to index. The index supports fields from the `TreeNode` object and any custom fields defined using the [field editor](https://docs.xperience.io/x/RIXWCQ).

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

> :ab: The property names (and names used in the [SourceAttribute](#source-attribute)) are __case-insensitive__. This means that your search model can contain an "articletext" property, or an "ArticleText" property - both will work.

Indexes must be registered during application startup. To register an index, modify the `AddAlgolia()` method called in __Program.cs__. This method accepts a list of [`AlgoliaIndex`](/src/Models/AlgoliaIndex.cs) instances, allowing you to create and register as many indexes as needed:

```cs
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), SiteSearchModel.IndexName),
    // Additional index registrations as needed...
});
```

If you're developing your search solution in multiple environments (e.g. "DEV" and "STG"), we recommended that you create a unique Algolia index per environment. With this approach, the search functionality can be tested in each environment individually and changes to the index structure or content will not affect other environments. This can be implemented any way you'd like, including some custom service which transforms the index names. The simplest approach is to prepend the environment name, stored in the application settings, to the index:

```json
--- appsettings.json ---

// Stores the name of the current environment
"Environment": "DEV",
```

```cs
--- Program.cs ---

var environment = builder.Configuration["Environment"];
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), $"{environment}-{SiteSearchModel.IndexName}")
});
```

### Determining which pages to index

While the above sample code will create an Algolia index, pages in the content tree will not be indexed until one or more [`IncludedPathAttribute`](https://github.com/Kentico/xperience-algolia/blob/master/src/Attributes/IncludedPathAttribute.cs) attributes are applied to the class. The `IncludedPathAttribute` has two properties to configure:

- __AliasPath__: The path of the content tree to index. Use wildcard "/%"  to index all children of a page.
- __ContentTypes__ (optional): The code names of the Page content types under the specified [`AliasPath`](https://docs.xperience.io/x/4obWCQ#Retrievepagecontent-Pagepathexpressions) to index. If not provided, all content types are indexed.

> :bulb: We recommend using [generated code files](https://docs.xperience.io/x/5IbWCQ) to reference Page content type class names.

The code sample below demonstrates using `IncludedPathAttribute` to include multiple paths and Page content types in an index:

```cs
[IncludedPath("/Articles/%", ContentTypes = new string[] { Article.CLASS_NAME })]
[IncludedPath("/Coffees/%", ContentTypes = new string[] { Coffee.CLASS_NAME })]
public class SiteSearchModel : AlgoliaSearchModel
```

### Customizing the indexing process

In some cases, you may want to customize the values that are sent to Algolia during the indexing process. For example, the [sample `SiteSearchModel`](#gear-creating-and-registering-an-algolia-index) search model above contains the `Content` property which retrieves its value from the `ArticleText` or `CoffeeDescription` fields. However, the content of your pages may be retrieved from [linked content items](https://docs.xperience.io/xp/developers-and-admins/development/content-modeling/content-types#Contenttypes-Addoptiontolinkcontentitems) instead.

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

## :memo: Configuring Algolia attributes

The integration package includes five attributes which can be applied to individual Algolia index attributes to further configure the Algolia index:

- [`Searchable`](#searchable-attribute)
- [`Facetable`](#facetable-attribute)
- [`Retrievable`](#retrievable-attribute)
- [`Source`](#source-attribute)
- [`MediaUrls`](#mediaurls-attribute)

### __Searchable__ attribute

This attribute indicates that an Algolia index attribute is [searchable](https://www.algolia.com/doc/api-reference/api-parameters/searchableAttributes/#how-to-use). You can define optional attribute properties to adjust the performance of your searchable index attributes:

- __Order__ (optional): Index attributes with lower `Order` will be given priority when searching for text. Index attributes without `Order` set will be added to the end of the list (making them lowest priority), while attributes with the same `Order` will be added with the same priority and are automatically `Unordered`.
- __Unordered__ (optional): By default, matches at the beginning of a text are more relevant than matches at the end of the text. If set to `true`, the position of the matched text in the attribute content is irrelevant.

```cs
[Searchable]
public string DocumentName { get; set; }

[Searchable(Order = 0)] // Highest priority
public string DocumentName { get; set; }

[Searchable(Unordered = true)]
public string DocumentName { get; set; }
```

### __Facetable__ attribute

This attribute indicates that an Algolia index attribute is a [facet or filter](https://www.algolia.com/doc/api-reference/api-parameters/attributesForFaceting/#how-to-use). By creating facets, your developers are able to create a [faceted search](https://www.algolia.com/doc/guides/managing-results/refine-results/faceting/) interface on the front-end application. Optional attribute properties can be defined to change the functionality of your faceted index attributes:

- __FilterOnly__ (optional): Defines the attribute as a filter and not a facet. If you do not need facets, defining an attribute as a filter reduces the size of the index and improves the speed of the search.

- __Searchable__ (optional): Allows developers to search for values within a facet, e.g. via the [`SearchForFacetValues()`](https://www.algolia.com/doc/api-reference/api-methods/search-for-facet-values/) method.

- __UseAndCondition__ (optional): When using the sample code in this repository and the `AlgoliaFacetFilterViewModel` class, facet conditions of the same properties are joined by "OR" by default. For example, `(CoffeProcessing:washed OR CoffeeProcessing:natural)`. You may set this property to __true__ to join them by "AND" instead.

> :warning: A property cannot be both `FilterOnly` and `Searchable`, otherwise an exception will be thrown.

```cs
[Facetable]
public string CoffeeProcessing { get; set; }

[Facetable(FilterOnly = true)] // Filter
public string CoffeeProcessing { get; set; }

[Facetable(Searchable = true)] // Searchable
public string CoffeeProcessing { get; set; }
```

### __Retrievable__ attribute

This attribute determines which Algolia index attributes to [retrieve when searching](https://www.algolia.com/doc/api-reference/api-parameters/attributesToRetrieve/#how-to-use). Reducing the number of retrieved index attributes helps improve the speed of your searches without impacting the search functionality.

```cs
[Searchable, Retrievable] // Used during search and retrieval
public string DocumentName { get; set; }

[Searchable] // Used when searching but not during retrieval
public string ArticleText { get; set; }
```

### __Source__ attribute

You can use the `Source` attribute to specify which content type fields are stored in a given Algolia index attribute.

By default, the value stored in the index attribute is retrieved from the content type field that matches the name of the declared property in the search model class. 

In certain cases however, you may wish to include content from multiple fields under a single index attribute. For example, if your project doesn't use uniform field naming conventions across content types, or you need to index multiple fields from a single content type under one index attribute.

```cs
// Ensures the 'Content' index attribute contains values from both the 'ArticleText' and 'CoffeeDescription' fields 
[Searchable, Source(new string[] { nameof(Article.ArticleText), nameof(Coffee.CoffeeDescription) })]
public string Content { get; set; }
```

Fields specified in the `Source` attribute are parsed in the order they appear, until a non-empty string and non-null value is found, which is then indexed. When referencing content type fields, use the `nameof()` expression to avoid typos.

### __MediaUrls__ attribute

This attribute is intended for fields that use the Xperience ["Media files" data type](https://docs.xperience.io/x/RoXWCQ). When the page is indexed, the files are converted into a list of live-site URLs.

```cs
[MediaUrls, Retrievable]
public string ArticleTeaser { get; set; }

[MediaUrls, Retrievable, Source(new string[] { nameof(Article.ArticleTeaser), nameof(Coffee.CoffeeImage) })]
public IEnumerable<string> Thumbnail { get; set; }
```

## :scissors: Splitting large content

Due to [limitations](https://support.algolia.com/hc/en-us/articles/4406981897617-Is-there-a-size-limit-for-my-index-records-/) on the size of Algolia records, we recommend splitting large content into smaller fragments. This operation is performed automatically during indexing by [`IAlgoliaObjectGenerator.SplitData()`](/src/Services/IAlgoliaObjectGenerator.cs), but there is no data splitting by default.

To enable data splitting for an Algolia index, add the `DistinctOptions` parameter during registration:

```cs
builder.Services.AddAlgolia(builder.Configuration, new AlgoliaIndex[]
{
    new AlgoliaIndex(typeof(SiteSearchModel), SiteSearchModel.IndexName, new DistinctOptions(nameof(SiteSearchModel.DocumentName), 1))
});
```

The `DistinctOptions` constructor accepts two parameters:

  - __distinctAttribute__: Corresponds with [this Algolia setting](https://www.algolia.com/doc/api-reference/api-parameters/attributeForDistinct). This is a property of the search model whose value will remain constant for all fragments, and is used to identify fragments during de-duplication. Fragments of a search result are "grouped" together according to this attribute's value, then a certain number of fragments per-group are returned, depending on the `distinctLevel` setting. In most cases, this will be a property like `DocumentName` or `NodeAliasPath`.
  - __distinctLevel__: Corresponds with [this Algolia setting](https://www.algolia.com/doc/api-reference/api-parameters/distinct). A value of zero disables de-duplication and grouping, while positive values determine how many fragments will be returned by a search. This is generally set to "1" so that only one fragment is returned from each grouping.

To implement data splitting, create and register a custom implementation of `IAlgoliaObjectGenerator`. It's __very important__ to set the "objectID" of each fragment, as seen in the example below. The IDs can be any arbitrary string, but setting this ensures that the fragments are updated and deleted properly when the page is modified. We recommend developing a consistent naming strategy like in the example below, where an index number is appended to the original ID. The IDs ___must not___ be random! Calling `SplitData()` on the same node multiple times should always generate the same fragments and IDs.

In the following example, we have large articles on our website which can be split into smaller fragments by splitting text on the `<p>` tag. Note that each fragment still contains all of the original data- only the "Content" property is modified.

```cs
[assembly: RegisterImplementation(typeof(IAlgoliaObjectGenerator), typeof(CustomAlgoliaObjectGenerator))]
namespace DancingGoat.Algolia
{
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
}
```

## :mag_right: Implementing the search interface

You can use Algolia's [.NET API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/csharp/?client=csharp), [JavaScript API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/javascript/?client=javascript), or [InstantSearch.js](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) to implement a search interface on your live site. The following example will help you with creating a search interface for .NET Core. In your Controllers, you can get a `SearchIndex` object by injecting the `IAlgoliaIndexService` interface and calling the `InitializeIndex()` method using your index's code name. Then, construct a `Query` to search the Algolia index. Algolia's pagination is zero-based, so in the Dancing Goat sample project we subtract 1 from the current page number:

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

The properties of each hit are populated from the Algolia index, but be sure not to omit `null` checks when working with the results. For example, a property that does _not_ have the [`Retrievable`](#retrievable-attribute) attribute is not returned and custom content type fields are only present for results of that type. That is, a property named "ArticleText" will be `null` for the coffee pages in Dancing Goat. You can reference the [`SiteSearchModel.ClassName`](/src/Models/AlgoliaSearchModel.cs) property present on all indexes to check the type of the returned hit.

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

### Creating an autocomplete search box

Algolia provides [autocomplete](https://www.algolia.com/doc/ui-libraries/autocomplete/introduction/what-is-autocomplete/) functionality via javascript which you can [install](https://www.algolia.com/doc/ui-libraries/autocomplete/introduction/getting-started/#installation) and set up according to your preferences. Below is an example of how to add autocomplete functionality to the Dancing Goat sample site.

1. In the `/_Layout.cshtml` view which is rendered for every page, add a reference to Algolia's scripts and the default theme for autocomplete:

```cshtml
<script src="//cdn.jsdelivr.net/algoliasearch/3/algoliasearch.min.js"></script>
<script src="//cdn.jsdelivr.net/autocomplete.js/0/autocomplete.min.js"></script>
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@@algolia/autocomplete-theme-classic"/>
```

2. Add an input for the autocomplete search box under the `<ul class="additional-menu">` element:

```html
<li class="search-menu-item">
    <div class="searchBox">
        <input id="search-input" placeholder="Search">
    </div>
</li>
```

3. Load the Algolia keys from `appsettings.json`:

```cshtml
@using Microsoft.Extensions.Options
@inject IOptions<AlgoliaOptions> options

@{
    var algoliaOptions = options.Value;
}
```

4. Add a script near the end of the `<body>` which loads your Algolia index. Be sure to use your __Search API Key__ which is public!

```js
<script type="text/javascript">
    var client = algoliasearch('@algoliaOptions.ApplicationId', '@algoliaOptions.SearchKey');
    var index = client.initIndex('@SiteSearchModel.IndexName');
</script>
```

5. Initialize the autocomplete search box, then create a handler for when users click on autocomplete suggestions, and when the _Enter_ button is used:

```js
var autocompleteBox = autocomplete('#search-input', {hint: false}, [
{
    source: autocomplete.sources.hits(index, {hitsPerPage: 5}),
    displayKey: 'DocumentName' // The Algolia attribute used to display the title of a suggestion
}
]).on('autocomplete:selected', function(event, suggestion, dataset) {
	window.location = suggestion.Url; // Navigate to the clicked suggestion
});

document.querySelector("#search-input").addEventListener("keyup", (e) => {
	if (e.key === 'Enter') {
        // Navigate to search results page when Enter is pressed
        var searchText = document.querySelector("#search-input").value;
        window.location = '@(Url.Action("Index", "Search"))?searchtext=' + searchText;
    }
});
```

When you build and run the Dancing Goat website and start typing into the search box, records from the Algolia index will be suggested:

![Autocomplete](/img/autocomplete-default-theme.png)

#### Customizing the autocomplete search box

In our sample implementation of the Algolia autocomplete search box,the standard [Autocomplete classic theme](https://www.algolia.com/doc/ui-libraries/autocomplete/introduction/getting-started/#install-the-autocomplete-classic-theme) was used for basic styling of the search box and the autocomplete suggestion layout. You can reference the theme's [CSS classes and variables](https://www.algolia.com/doc/ui-libraries/autocomplete/api-reference/autocomplete-theme-classic/) to customize the appearance of the search box to match the design of your website.

In the Dancing Goat website, you can add the following to the CSS which styles the search box and suggestions to match the Dancing Goat theme:

```css
/*# Algolia search box #*/
.searchBox .aa-dropdown-menu {
    background-color: #fff;
    padding: 5px;
    top: 120% !important;
    width: 100%;
    box-shadow: 0 1px 0 0 rgba(0, 0, 0, 0.2), 0 2px 3px 0 rgba(0, 0, 0, 0.1);
}
.searchBox .algolia-autocomplete {
    width: 100%;
}

.searchBox .aa-input {
    width: 100%;
    background-color: transparent;
    padding-left: 10px;
    padding-top: 5px;
}

.searchBox .aa-suggestion {
    padding: 5px 5px 0;
}

.searchBox .aa-suggestion em {
    color: #4098ce;
}

.searchBox .aa-suggestion.aa-cursor {
    background: #eee;
    cursor: pointer;
}
```

The layout of each individual suggestion can be customized by providing a [custom template](https://www.algolia.com/doc/ui-libraries/autocomplete/core-concepts/templates/) in the `autocomplete()` function. In the Dancing Goat website, you can add an image to each suggestion and highlight the matching search term by adding the following to your script:

```js
var autocompleteBox = autocomplete('#search-input', {hint: false}, [
{
    source: autocomplete.sources.hits(index, {hitsPerPage: 5}),
    templates: {
        suggestion: (item) =>
            `<img style='width:40px;margin-right:10px' src='${item.Thumbnail}'/><span>${item._highlightResult.DocumentName.value}</span>`
    }
}
```

> :warning: The attributes `DocumentName` and `Thumbnail` used in this example are not present in all Algolia indexes! If you follow this example, make sure you are using attributes present in your index. See the [sample search model](#gear-creating-and-registering-an-algolia-index) to find out how these attributes were defined.

## :ballot_box_with_check: Faceted search

As the search interface can be designed in multiple languages using Algolia's APIs, your developers can implement [faceted search](https://www.algolia.com/doc/guides/managing-results/refine-results/faceting/). However, this repository contains some helpful classes to develop faceted search using C#. The following is an example of creating a faceted search interface within the Dancing Goat sample site's coffee section.

### Setting up basic search

The Dancing Goat site doesn't use search out-of-the-box, so first you need to hook it up to Algolia. In this example, the search model seen [here](#gear-creating-and-registering-an-algolia-index) is used.

1. Inject `IAlgoliaIndexService` into the `CoffeesController` as shown in [this section](#magright-implementing-the-search-interface).

2. In __CoffeesController.cs__, create a method that performs a standard Algolia search. In the `Query.Filters` property, add a filter to only retrieve records where `ClassName` is `DancingGoatCore.Coffee.` You also specify which `Facets` you want to retrieve, but they are not used yet.

```cs
private async Task<SearchResponse<SiteSearchModel>> Search(CancellationToken cancellationToken)
{
    var facetsToRetrieve = new string[] {
        nameof(SiteSearchModel.CoffeeIsDecaf),
        nameof(SiteSearchModel.CoffeeProcessing)
    };

    var defaultFilter = $"{nameof(SiteSearchModel.ClassName)}:{new Coffee().ClassName}";
    var query = new Query()
    {
        Filters = defaultFilter,
        Facets = facetsToRetrieve
    };

    var searchIndex = await algoliaIndexService.InitializeIndex(SiteSearchModel.IndexName, cancellationToken);
    return await searchIndex.SearchAsync<SiteSearchModel>(query, ct: cancellationToken);
}
```

3. Create a new `CoffeeSearchViewModel` class which we will pass to our view.

```cs
public class CoffeeSearchViewModel
{
    public IEnumerable<CoffeeViewModel> Coffees { get; set; }

    public IAlgoliaFacetFilter Filter { get; set; }
}
```

4. Modify the `Index()` method to perform the search and provide the list of hits converted into `CoffeeViewModel` objects:

```cs
public async Task<IActionResult> Index(CancellationToken cancellationToken)
{
    var searchResponse = await Search(cancellationToken);
    var coffees = searchResponse.Hits.Select(hit => new CoffeeViewModel
    {
        Name = hit.DocumentName,
        Description = hit.ShortDescription,
        ImagePath = hit.Thumbnail.FirstOrDefault(),
        Url = hit.Url
    });

    return View(new CoffeeSearchViewModel
    {
        Coffees = coffees
    });
}
```

5. Modify the _\Views\Coffees\Index.cshtml_ file to accept our new `CoffeeSearchViewModel` and display the coffees.

### Filtering your search with facets

In the `Search()` method, the _CoffeeIsDecaf_ and _CoffeeProcessing_ facets are retrieved from Algolia, but they are not used yet. In the following steps you will use an `AlgoliaFacetFilter` (which implements `IAlgoliaFacetFilter`) to hold the facets and the current state of the faceted search interface. The `UpdateFacets()` method of this interface allows you convert the facet response into a list of `AlgoliaFacetedAttribute`s which contains the attribute name (e.g. "CoffeeIsDecaf"), localized display name (e.g. "Decaf"), and a list of `AlgoliaFacet` objects.

Each `AlgoliaFacet` object represents the faceted attribute's possible values and contains the number of results that will be returned if the facet is enabled. For example, the "CoffeeProcessing" `AlgoliaFacetedAttribute` contains 3 `AlgoliaFacet` objects in its `Facets` property.

1. In the Xperience administration, edit the "Coffee" content type and add the __CoffeeProcessing__ and __CoffeeIsDecaf__ fields with data types "Boolean" and "Text" respectively. For the "CoffeeProcessing" field, use the "Dropdown selector" component and add some values like "washed" and "natural."

2. In the __Pages__ application, edit the coffess and set values for the new fields.

3. In the `Search()` method, add a parameter that accepts `IAlgoliaFacetFilter`. Then, call the `GetFilter()` method to generate the facet filters:

```cs
private async Task<SearchResponse<SiteSearchModel>> Search(IAlgoliaFacetFilter filter, CancellationToken cancellationToken)
{
    var facetsToRetrieve = new string[] {
        nameof(SiteSearchModel.CoffeeIsDecaf),
        nameof(SiteSearchModel.CoffeeProcessing)
    };

    var defaultFilter = $"{nameof(SiteSearchModel.ClassName)}:{new Coffee().ClassName}";
    var facetFilter = filter.GetFilter(typeof(SiteSearchModel));
    if (!String.IsNullOrEmpty(facetFilter))
    {
        defaultFilter += $" AND {facetFilter}";
    }
    
    var query = new Query()
    {
        Filters = defaultFilter,
        Facets = facetsToRetrieve
    };

    var searchIndex = await algoliaIndexService.InitializeIndex(SiteSearchModel.IndexName, cancellationToken);
    return await searchIndex.SearchAsync<SiteSearchModel>(query, ct: cancellationToken);
}
```

The `GetFilter()` method returns a condition for each facet in the `IAlgoliaFacetFilter` which has the `IsChecked` property set to true. Facets with the same attribute name are grouped within an "OR" condition. For example, if a visitor on your store listing checked the boxes for decaf coffee with the "washed" and "natural" processing type, the filter will look like this:

> "CoffeeIsDecaf:true" AND ("CoffeeProcessing:washed" OR "CoffeeProcessing:natural")

You can change this behavior by setting the [`UseAndCondition`](#facetable-attribute) property of your faceted attributes, or by creating your own implementation of `IAlgoliaFacetFilter`.

4. Modify the `Index()` action to accept an `AlgoliaFacetFilter` parameter, pass it to the `Search()` method, parse the facets from the search response, then pass the filter to the view:

```cs
public async Task<IActionResult> Index(AlgoliaFacetFilter filter, CancellationToken cancellationToken)
{
    ModelState.Clear();
    var searchResponse = await Search(filter, cancellationToken);
    filter.UpdateFacets(new FacetConfiguration(searchResponse.Facets));

    var coffees = searchResponse.Hits.Select(hit => new CoffeeViewModel
    {
        Name = hit.DocumentName,
        Description = hit.ShortDescription,
        ImagePath = hit.Thumbnail.FirstOrDefault(),
        Url = hit.Url
    });

    return View(new CoffeeSearchViewModel
    {
        Coffees = coffees,
        Filter = filter
    });
}
```

Here, the `UpdateFacets()` method accepts the facets returned from Algolia. Because the entire list of available facets depends on the Algolia response, and the facets in your filter are replaced with new ones, this method ensures that a facet that was used previously (e.g. "CoffeeIsDecaf:true") maintains it's enabled state when reloading the search interface.

### Displaying the facets

The Dancing Goat store listing now uses Algolia search, and you have a filter which contains Algolia facets and properly filters the search results. The final step is to display the facets in the store listing and handle user interaction with the facets.

1. In _\Views\Coffees\Index.cshtml_, add a partial view to the existing `aside` element which will display the facets:

```html
<aside class="col-md-2 col-lg-3">
    <form asp-controller="Coffees" asp-action="Index">
        <partial name="~/Views/Shared/Algolia/_AlgoliaFacetFilter.cshtml" model="Model.Filter" />
    </form>
</aside>
```

2. Add a `scripts` section with Javascript that will reload the search interface when a facet is clicked:

```html
@section scripts {
    <script>
    $(function () {
        $('.js-postback input:checkbox').change(function () {
            $(this).parents('form').submit();
        });
    });
    </script>
}
```

3. Create the _/Views/Shared/Algolia/\_AlgoliaFacetFilter.cshtml_ view referenced in step #1. This view will accept our facet filter and loop through each `AlogliaFacetedAttribute` it contains:

```cshtml
@using Kentico.Xperience.Algolia.Models
@model IAlgoliaFacetFilter

@for (var i=0; i<Model.FacetedAttributes.Count(); i++)
{
    @Html.HiddenFor(m => Model.FacetedAttributes[i].Attribute)
    @Html.EditorFor(model => Model.FacetedAttributes[i], "~/Views/Shared/Algolia/EditorTemplates/_AlgoliaFacetedAttribute.cshtml")
}
```

4. For each `AlgoliaFacetedAttribute` you now want to loop through each `AlgoliaFacet` it contains and display a checkbox that will enable the facet for filtering. Create the _/Views/Shared/Algolia/EditorTemplates/\_AlgoliaFacetedAttribute.cshtml_ view and render inputs for each facet:

```cshtml
@using Kentico.Xperience.Algolia.Models
@model AlgoliaFacetedAttribute

<h4>@Model.DisplayName</h4>
@for (var i = 0; i < Model.Facets.Count(); i++)
{
    @Html.HiddenFor(m => Model.Facets[i].Value)
    @Html.HiddenFor(m => Model.Facets[i].Attribute)
    <span class="checkbox js-postback">
        <input data-facet="@(Model.Attribute):@Model.Facets[i].Value" asp-for="@Model.Facets[i].IsChecked" />
        <label asp-for="@Model.Facets[i].IsChecked">@Model.Facets[i].DisplayValue (@Model.Facets[i].Count)</label>
    </span>
}
```

Now, when you check one of the facets your JavaScript code will cause the form to post back to the `Index()` action. The `filter` parameter will contain the facets that were displayed on the page, with the `IsChecked` property of each facet set accordingly. The filter is passed to our `Search()` method which uses `GetFilter()` to filter the search results, and a new `AlgoliaFacetFilter` is created with the results of the query.

![Dancing goat facet example](/img/dg-facets.png)

### Translating facet names and values

Without translation, the view will display facet attribute names (e.g. "CoffeeIsDecaf") instead of a human-readable title like "Decaffeinated," and values like "true" and "false." The `FacetConfiguration` model accepted by `IAlgoliaFacetFilter.UpdateFacets()` contains the `displayNames` parameter which can be used to translate facets into any text you'd like.

1. Create a new class (or use an existing class) to hold a `Dictionary<string, string>` containing the translations:

```cs
public class AlgoliaFacetTranslations
```

2. Add entries to the dictionary with keys in the format _[AttributeName]_ or _[AttributeName].[Value]_ for faceted attributes or facet values, respectively:

```cs
  public static Dictionary<string, string> CoffeeTranslations
  {
      get
      {
          return new Dictionary<string, string>
          {
              { nameof(SiteSearchModel.CoffeeIsDecaf), "Decaffeinated" },
              { $"{nameof(SiteSearchModel.CoffeeIsDecaf)}.true", "Yes" },
              { $"{nameof(SiteSearchModel.CoffeeIsDecaf)}.false", "No" },
              { nameof(SiteSearchModel.CoffeeProcessing), "Processing" },
              { $"{nameof(SiteSearchModel.CoffeeProcessing)}.washed", "Washed" },
              { $"{nameof(SiteSearchModel.CoffeeProcessing)}.natural", "Natural" }
          };
      }
  }
```

3. Reference this dictionary when calling the `UpdateFacets()` method in your search interface:

```cs
var searchResponse = await Search(filter, cancellationToken);
filter.UpdateFacets(new FacetConfiguration(searchResponse.Facets, AlgoliaFacetTranslations.CoffeeTranslations));
```

## :bulb: Personalizing search results

Algolia provides search result [personalization](https://www.algolia.com/doc/guides/personalization/what-is-personalization/) that allows you to offer more relevant results to individual site visitors. To begin personalizing search results, you first need to send [events](https://www.algolia.com/doc/guides/sending-events/planning/) to Algolia which detail the visitor's activity. Sending events varies depending on your API of choice and how your search is implemented. You can choose to use any of the approaches in the Algolia documentation (e.g., [Google Tag Manager](https://www.algolia.com/doc/guides/sending-events/implementing/connectors/google-tag-manager/)). 

The following section demonstrates how to send events using C# with the assistance of some classes from this repository.


>If you do not already have a basic search interface set up, you need to [implement one](#mag_right-implementing-the-search-interface).

### Sending search result click events/conversions

To track these types of events, the `ClickAnalytics` property must be enabled when creating your search query:

```cs
var query = new Query(searchText)
{
    Page = page,
    HitsPerPage = PAGE_SIZE,
    ClickAnalytics = true
};
```

This repository uses query string parameters to track the required data for submitting search result clicks and conversion to Algolia. As all search models extend `AlgoliaSearchModel` and contain a `Url` property, you can call `IAlgoliaInsightsService.SetInsightsUrls()` to update the URL of your results with all the necessary data:

```cs
// Inject IAlgoliaInsightsService
public SearchController(IAlgoliaInsightsService algoliaInsightsService)
{
    _algoliaInsightsService = algoliaInsightsService;
}

// In your search method, call SetInsightsUrls
var results = await searchIndex.SearchAsync<SiteSearchModel>(query, ct: cancellationToken);
_algoliaInsightsService.SetInsightsUrls(results);
```

Now, when you display the search results using the `Url` property, it will look something like _https://mysite.com/search?object=88&pos=2&query=d057994ba21f0a56c75511c2c005f49f_. To submit the event to Algolia when your visitor clicks this link, inject `IAlgoliaInsightsService` into the view that renders the linked page. Or, you can inject it into the view which renders all pages, e.g. _\_Layout.cshtml_. Call `LogSearchResultClicked()`, `LogSearchResultConversion()`, or both methods of the service:

```cshtml
@inject IAlgoliaInsightsService _insightsService

@{
    await _insightsService.LogSearchResultClicked("Search result clicked", SiteSearchModel.IndexName, CancellationToken.None);
    await _insightsService.LogSearchResultConversion("Search result converted", SiteSearchModel.IndexName, CancellationToken.None);
}
```

When a visitor lands on a page after clicking on a search result, these methods use the data contained in the query string to submit a search result click event or conversion. If the visitor arrives on the page without query string parameters (e.g. using the site navigation), nothing is logged.

### Sending generic page-related events/conversions

Aside from search result related events/conversions, there are many more generic events you can send to Algolia. For example, for sites that produce blog posts or articles, you may want to send an _Article viewed_ event.

For a conversion, you can use the `IAlgoliaInsightsService.LogPageConversion()` method in your controllers or views:

```cs
public async Task<IActionResult> Detail([FromServices] ArticleRepository articleRepository)
{
    var article = articleRepository.GetCurrent();

    await _insightsService.LogPageConversion(article.DocumentID, "Article viewed", SiteSearchModel.IndexName, CancellationToken.None);

    return new TemplateResult(article);
}
```

You can also log an event when a visitor simply views a page with the `LogPageViewed()` method. For example, in the __ArticlesController__ you can log an _Article viewed_ event:

```cs
public async Task<IActionResult> Detail([FromServices] ArticleRepository articleRepository)
{
    var article = articleRepository.GetCurrent();

    await _insightsService.LogPageViewed(article.DocumentID, "Article viewed", SiteSearchModel.IndexName, CancellationToken.None);

    return new TemplateResult(article);
}
```

### Logging facet-related events/conversions

You can log events and conversions when facets are displayed to a visitor, or when they click on an individual facet. In this example, the code from our Dancing Goat faceted search [example](#filtering-your-search-with-facets) is used. Logging a _Search facets viewed_ event is done in the `Index()` action of __CoffeesController__. The `LogFacetsViewed()` method requires a list of `AlgoliaFacetedAttribute`s, which you can get from the filter:

```cs
var searchResponse = await Search(filter, cancellationToken);
filter.UpdateFacets(new FacetConfiguration(searchResponse.Facets));
await _insightsService.LogFacetsViewed(filter.FacetedAttributes, "Store facets viewed", SiteSearchModel.IndexName);
```

To log an event or conversion when a facet is clicked, you need to use AJAX. First, in the _\_AlgoliaFacetedAttribute.cshtml_ view which displays each check box, add a `data` attribute that stores the facet name and value (e.g. "CoffeeIsDecaf:true"):

```cshtml
<input data-facet="@(Model.Attribute):@Model.Facets[i].Value" asp-for="@Model.Facets[i].IsChecked" />
```

In the _Index.cshtml_ view for the coffee listing, the `change()` function is already used to run some JavaScript when a facet is checked or unchecked. Let's add code that runs only if the facet has been checked which gets the value of the new `data` attribute and sends a POST request:

```js
<script>
    $(function () {
        $('.js-postback input:checkbox').change(function () {
            if($(this).is(':checked')) {
                var facet = $(this).data('facet');
                fetch('@Url.Action("FacetClicked", "Coffees")?facet='+facet, {
                    method: 'POST'
                });
            }

            $(this).parents('form').submit();
        });
    });
</script>
```

This sends the request to the __CoffeesController__ `FacetClicked()` action, but you can send the request anywhere else. Check the __Program.cs__ to make sure your application can handle this request:

```cs
app.MapControllerRoute(
    name: "facetClicked",
    pattern: "Algolia/FacetClicked/{facet?}",
    defaults: new { controller = "Coffees", action = "FacetClicked" }
);
```

In the appropriate controller, create the action which accepts the facet parameter and logs the event, conversion, or both:

```cs
[HttpPost]
public async Task<ActionResult> FacetClicked(string facet)
{
    if (String.IsNullOrEmpty(facet))
    {
        return BadRequest();
    }

    await _insightsService.LogFacetClicked(facet, "Store facet clicked", AlgoliaSiteSearchModel.IndexName, CancellationToken.None);
    await _insightsService.LogFacetConverted(facet, "Store facet converted", AlgoliaSiteSearchModel.IndexName, CancellationToken.None);
    return Ok();
}
```

### Configuring Personalization

Once you've begun to track events using the examples in the previous sections, you can configure a [personalization strategy](https://www.algolia.com/doc/guides/personalization/personalizing-results/in-depth/configuring-personalization/). This is done directly in the Algolia interface, in your application's __Personalization__ menu.

After your Personalization strategy is configured, you must set certain properties during your search queries to retrieve personalized results:

- __EnablePersonalization__
- __UserToken__: A token which identifies the visitor performing the search. Using the code and examples in this repository, the user token will be the current contact's GUID.
- __X-Forwarded-For__ header: The IP address of the visitor performing the search. See [Algolia's documentation](https://www.algolia.com/doc/guides/getting-analytics/search-analytics/out-of-the-box-analytics/how-to/specify-which-user-is-doing-the-search/#set-the-x-forwarded-for-header).

```cs
var query = new Query(searchText)
{
    Page = page,
    HitsPerPage = PAGE_SIZE,
    ClickAnalytics = true,
    EnablePersonalization = true,
    UserToken = ContactManagementContext.CurrentContact.ContactGUID.ToString()
};
var results = await searchIndex.SearchAsync<AlgoliaSiteSearchModel>(query, new RequestOptions {
    Headers = new Dictionary<string, string> { { "X-Forwarded-For", Request.HttpContext.Connection.RemoteIpAddress.ToString() } }
}, cancellationToken);
```

## :crystal_ball: Using InstantSearch.js

`InstantSearch.js` is a vanilla javascript library developed by Algolia which utilizes highly-customizable widgets to easily develop a search interface with nearly no coding. In this example, we will use `InstantSearch.js` in the Dancing Goat sample site with very few changes, using the search model sample code [here](#determining-the-pages-to-index).

1. Create a new empty Controller to display the search (e.g. __InstantsearchController__), and ensure it has a proper route in `Program.cs`:

```cs
endpoints.MapControllerRoute(
    name: "instantsearch",
    pattern: "Algolia/Instantsearch",
    defaults: new { controller = "Instantsearch", action = "Index" }
);
```

2. Create the _Index.cshtml_ view for your controller with the basic layout and stylesheet references. Load your Algolia settings for use later:

```cshtml
@using Microsoft.Extensions.Options
@inject IOptions<AlgoliaOptions> options

@{
    var algoliaOptions = options.Value;
}

@section styles {
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/instantsearch.css@7/themes/algolia-min.css" />
    <link rel="stylesheet" href="~/Content/Styles/instantsearch.css" />
}

<div class="row instantsearch-container">
    <div class="left-panel">
        <div id="clear-refinements"></div>

        <h2>Decaf</h2>
        <div id="decaf-list"></div>

        <h2>Processing</h2>
        <div id="processing-list"></div>

        <h2>Type</h2>
        <div id="type-list"></div>
    </div>
    <div class="right-panel">
        <div id="searchbox" class="ais-SearchBox"></div>
        <div id="hits"></div>
        <div id="pagination"></div>
    </div>
</div>
```

3. At the bottom of your view, add a `scripts` section which loads the InstantSearch.js scripts, initializes the search widget, three faceting widgets, the results widget, and pagination widget:

```cshtml
@section scripts {
    <script src="https://cdn.jsdelivr.net/npm/algoliasearch@4/dist/algoliasearch-lite.umd.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/instantsearch.js@4"></script>
    <script type="text/javascript">
        const search = instantsearch({
          indexName: '@SiteSearchModel.IndexName',
          searchClient: algoliasearch('@algoliaOptions.ApplicationId', '@algoliaOptions.SearchKey'),
        });

        search.addWidgets([
          instantsearch.widgets.searchBox({
            container: '#searchbox',
          }),
          instantsearch.widgets.clearRefinements({
            container: '#clear-refinements',
          }),
          instantsearch.widgets.refinementList({
            sortBy: ['name:asc'],
            container: '#type-list',
            attribute: 'ClassName',
          }),
          instantsearch.widgets.refinementList({
            sortBy: ['name:asc'],
            container: '#processing-list',
            attribute: 'CoffeeProcessing',
          }),
          instantsearch.widgets.refinementList({
            sortBy: ['name:asc'],
            container: '#decaf-list',
            attribute: 'CoffeeIsDecaf',
          }),
          instantsearch.widgets.hits({
            container: '#hits',
            templates: {
              item: renderHitTemplate,
            },
          }),
          instantsearch.widgets.pagination({
            container: '#pagination',
          }),
        ]);

        search.start();

        function renderHitTemplate(item) {
            return `<div>
                <a href="${item.Url}">
                    <img src="${item.Thumbnail}" align="left" alt="${item.DocumentName}" />
                </a>
                <div class="hit-name">
                <a href="${item.Url}">
                    ${item.DocumentName}
                </a>
                </div>
                <div class="hit-description">
                ${item.ShortDescription}
                </div></div>`;
        }
    </script>
}
```

4. Create the _/wwwroot/Content/Styles/instantsearch.css_ stylesheet which overrides some default _InstantSearch.js_ styling to fit the Dancing Goat theme:

```css
.instantsearch-container {
    padding: 20px;
}

.ais-ClearRefinements {
    margin: 1em 0;
}

.ais-SearchBox {
    margin: 1em 0;
    width:  97%;
}

.ais-Pagination {
    margin-top: 1em;
}

.ais-Pagination-item--selected .ais-Pagination-link {
    color: #fff;
    background-color: #272219;
    border-color: #272219;
}

.left-panel {
    float: left;
    width: 290px;
}

.right-panel {
    margin-left: 310px;
}

.ais-InstantSearch {
    max-width: 960px;
    overflow: hidden;
    margin: 0 auto;
}

.ais-RefinementList-count {
    color: #fff;
    background-color: #272219;
}

.ais-ClearRefinements-button {
    background-color: #272219;
}

.ais-ClearRefinements-button--disabled:focus, .ais-ClearRefinements-button--disabled:hover {
    background-color: #272219;
    opacity: 0.9;
}

.ais-Hits-item {
    background-color: #fff;
    margin-bottom: 1em;
    width: calc(50% - 1rem);
    border-radius: 5px;
    border: 0px;
}

.ais-Hits-item img {
    margin-right: 1em;
    width: 100px;
}

.hit-name {
    margin-bottom: 0.5em;
}

.hit-description {
    color: #888;
    font-size: 14px;
    margin-bottom: 0.5em;
}
```

When you run the site and visit your new page, you'll see that you have a fully functioning search interface with faceting. See Algolia's [InstantSearch documentation](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) for more detailed walkthroughs on designing the search interface and customizing widgets.

## :snail: Using Algolia crawlers

This integration provides basic support for [Algolia crawlers](https://www.algolia.com/doc/tools/crawler/getting-started/overview/). Crawlers are created and configured within [Algolia's Crawler Admin](https://crawler.algolia.com/admin), and this integration ensures that updated, archived, and deleted pages in Xperience are properly updated within the crawler.

### Limitations

The [endpoint](https://www.algolia.com/doc/rest-api/crawler/#crawl-specific-urls) used to request re-crawling of updated Xperience pages has a limitation of 200 requests per day. By default, the process which requests re-crawling of pages runs every 10 minutes (114 times per day) and the limitation shouldn't be reached when a _single_ crawler is registered. However, if you have registered multiple crawlers, you will need to extend the interval by setting the `crawlerInterval` setting in `appsettings.json`:

```json
"xperience.algolia": {
    "crawlerInterval": 1200000 // 20 minutes
}
```

### Setup

After you have created your crawler in Algolia, follow these steps to register the crawler in Xperience:

1. Locate the User ID and API Key in the [Algolia Crawler Admin](https://www.algolia.com/doc/tools/crawler/apis/crawler-rest-api/#authentication) and add the values to your `appsettings.json`:

```json
"xperience.algolia": {
    "crawlerUserId": "<Crawler User ID>",
    "crawlerApiKey": "<Crawler API Key>"
}
```

> :warning: Even if you are only using Algolia crawlers, you still need to include the application settings mentioned in step #4 of [Installation](#rocket-installation)

3. Locate the ID of the crawlers you wish to register in Xperience. The ID of each crawler can be found in the URL while navigating the Algolia Crawler Admin, or in the __Settings__ menu.  
4. In `Program.cs`, edit (or add) the `AddAlgolia` method to include one or more crawler IDs:

```cs
builder.Services.AddAlgolia(builder.Configuration, crawlers: new string[]
{
    "<Crawler ID>"
});
```

Your Xperience by Kentico application will now request re-crawling of all published pages in the content tree, and will delete records from the crawler when a page is deleted or archived.

### Configuring crawlers

As the data indexed by your crawler is managed entirely by Algolia, you are welcome to configure the crawler however you'd like using [Algolia's Editor](https://www.algolia.com/doc/tools/crawler/getting-started/crawler-configuration/#how-do-you-access-a-crawler-configuration). However, the "objectID" of your records __must__ be the URL of your pages! This is the default configuration, so you only need to ensure that it isn't changed. Below is a sample `actions` section of the configuration used in the Dancing Goat sample site:

```js
actions: [
    {
      indexName: "Dancing Goat",
      pathsToMatch: [
        "https://mysite.com/coffees/**",
        "https://mysite.com/articles/**",
      ],
      recordExtractor: ({ url, $, contentLength, fileType }) => {
        return [
          {
            objectID: url.href, // Do not change this!
            path: url.pathname.split("/")[1],
            fileType,
            title: $("head > title").text(),
            keywords: $("meta[name=keywords]").attr("content"),
            description: $("meta[name=description]").attr("content"),
            image: $('meta[property="og:image"]').attr("content"),
            content: $("p").text(),
          },
        ];
      },
    },
  ],
```

### Searching your crawler

As your crawler can contain any number of dynamic fields in its configuration, this integration doesn't contain a strongly-typed model for crawlers. We encourage your developers to create their own model for each crawler- using the example configuration above, the model could look like this:

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

In the below example we've only registered a single crawler, so we can use `FirstOrDefault()` to get the crawler ID. In cases where there are multiple crawlers registered, the developers need to create a mapping to identify which crawler is used in a particular search. We are also using the `path` and `fileType` attributes to only return pages under the /coffees path:

```cs
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
```

## :computer: Algolia application for the Xperience administration

After [installing](#rocket-installation) the NuGet package in your Xperience by Kentico project, a new _Algolia_ application becomes available in the __Development__ section. The application displays a table of all registered Algolia indexes with information about the number of records, build time, and last update:

![Algolia main menu](/img/main-menu.png)

Use the __Rebuild__ action on the right side of the table to re-index the pages of the Algolia index. This completely removes the existing records and replaces them with the most up-to-date data. Rebuilding indexes is especially useful after enabling the [data splitting](#scissors-splitting-large-content) feature. Selecting an index form the list displays a page detailing the indexed paths and properties of the corresponding Algolia index:

![Algolia indexed content menu](/img/indexed-content-menu.png)

The __Indexed properties__ table lists each property defined in the search model and the [attributes](#memo-configuring-algolia-attributes) of that property. 

The __Indexed paths__ table lists the search model's [`IncludedPathAttribute`s](#determining-which-pages-to-index), including the paths and content types included within each index attribute. 
Selecting an indexed path displays each content type included in the indexed path:

![Path detail menu](/img/path-detail-menu.png)

## Questions & Support

See the [Kentico home repository](https://github.com/Kentico/Home/blob/master/README.md) for more information about the product(s) and general advice on submitting questions.


## Contributing

For Contributing please see  <a href="./CONTRIBUTING.md">`CONTRIBUTING.md`</a> for more information.

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.
