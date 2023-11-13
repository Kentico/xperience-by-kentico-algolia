# Search Personalization

## :bulb: Personalizing search results

Algolia provides search result [personalization](https://www.algolia.com/doc/guides/personalization/what-is-personalization/) that allows you to offer more relevant results to individual site visitors. To begin personalizing search results, you first need to send [events](https://www.algolia.com/doc/guides/sending-events/planning/) to Algolia which detail the visitor's activity. Sending events varies depending on your API of choice and how your search is implemented. You can choose to use any of the approaches in the Algolia documentation (e.g., [Google Tag Manager](https://www.algolia.com/doc/guides/sending-events/implementing/connectors/google-tag-manager/)).

The following section demonstrates how to send events using C# with the assistance of some classes from this repository.

> If you do not already have a basic search interface set up, you need to [implement one](#mag_right-implementing-the-search-interface).

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

You can also log an event when a visitor simply views a page with the `LogPageViewed()` method. For example, in the **ArticlesController** you can log an _Article viewed_ event:

```cs
public async Task<IActionResult> Detail([FromServices] ArticleRepository articleRepository)
{
    var article = articleRepository.GetCurrent();

    await _insightsService.LogPageViewed(article.DocumentID, "Article viewed", SiteSearchModel.IndexName, CancellationToken.None);

    return new TemplateResult(article);
}
```

### Logging facet-related events/conversions

You can log events and conversions when facets are displayed to a visitor, or when they click on an individual facet. In this example, the code from our Dancing Goat faceted search [example](#filtering-your-search-with-facets) is used. Logging a _Search facets viewed_ event is done in the `Index()` action of **CoffeesController**. The `LogFacetsViewed()` method requires a list of `AlgoliaFacetedAttribute`s, which you can get from the filter:

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

This sends the request to the **CoffeesController** `FacetClicked()` action, but you can send the request anywhere else. Check the **Program.cs** to make sure your application can handle this request:

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

Once you've begun to track events using the examples in the previous sections, you can configure a [personalization strategy](https://www.algolia.com/doc/guides/personalization/personalizing-results/in-depth/configuring-personalization/). This is done directly in the Algolia interface, in your application's **Personalization** menu.

After your Personalization strategy is configured, you must set certain properties during your search queries to retrieve personalized results:

- **EnablePersonalization**
- **UserToken**: A token which identifies the visitor performing the search. Using the code and examples in this repository, the user token will be the current contact's GUID.
- **X-Forwarded-For** header: The IP address of the visitor performing the search. See [Algolia's documentation](https://www.algolia.com/doc/guides/getting-analytics/search-analytics/out-of-the-box-analytics/how-to/specify-which-user-is-doing-the-search/#set-the-x-forwarded-for-header).

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
