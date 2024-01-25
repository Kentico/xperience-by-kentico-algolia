# Search index querying

## Rebuild the Search Index

Each index will initially be empty after creation until you create or modify some content.

To index all existing content, rebuild the index in Xperience's Administration within the Search application added by this library.

## Create a search result model

```csharp
public class DancingGoatSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
```

## Create a search service

Execute a search with a customized Algolia `Query` using the IAlgoliaSearchService. Specify Facets or other special properties which you have defined in `GetAlgoliaIndexSettings` method in the custom AlgoliaIndexingStrategy

```csharp
public class SearchService
{
     private readonly IAlgoliaIndexService algoliaSearchService;

  public SearchService(IAlgoliaIndexService algoliaSearchService) => this.algoliaSearchService = algoliaSearchService;

  public async Task<SearchResponse<DancingGoatSearchResultModel>> GlobalSearch(
      string indexName,
      string searchText,
      int page = 1,
      int pageSize = 10,
      string facet = null)
  {
      var index = await algoliaSearchService.InitializeIndex(indexName, default);

      page = Math.Max(page, 1);

      var query = new Query(searchText)
      {
          Page = page - 1,
          HitsPerPage = pageSize
      };
      if (facet is not null)
      {
          query.Facets = new List<string> { nameof(DancingGoatSearchResultModel.ContentTypeName) };
      }

      var results = await index.SearchAsync<DancingGoatSearchResultModel>(query);

      return results;
  }
}
```

## Display Results

... TODO
