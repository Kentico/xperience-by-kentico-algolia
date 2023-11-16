# Using InstantSearch.js

`InstantSearch.js` is a vanilla javascript library developed by Algolia which utilizes highly-customizable widgets to easily develop a search interface with nearly no coding. In this example, we will use `InstantSearch.js` in the Dancing Goat sample site with very few changes, using the [search model sample code](./Usage-Guide.md#determining-which-pages-to-index).

1. Create a new empty Controller to display the search (e.g. `InstantsearchController`), and ensure it has a proper route in `Program.cs`:

    ```cs
    endpoints.MapControllerRoute(
        name: "instantsearch",
        pattern: "Algolia/Instantsearch",
        defaults: new { controller = "Instantsearch", action = "Index" }
    );
    ```

2. Create the `Index.cshtml` view for your controller with the basic layout and stylesheet references. Load your Algolia settings for use later:

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

4. Create the `~/wwwroot/Content/Styles/instantsearch.css` stylesheet which overrides some default InstantSearch.js styling to fit the Dancing Goat theme:

    ```css
    .instantsearch-container {
      padding: 20px;
    }

    .ais-ClearRefinements {
      margin: 1em 0;
    }

    .ais-SearchBox {
      margin: 1em 0;
      width: 97%;
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

    .ais-ClearRefinements-button--disabled:focus,
    .ais-ClearRefinements-button--disabled:hover {
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
