using System;
using System.Threading.Tasks;
using Algolia.Search.Models.Search;
using DancingGoat.Search.Models;
using Kentico.Xperience.Algolia.Indexing;

namespace DancingGoat.Search.Services;

public class SimpleSearchService
{
    private readonly IAlgoliaIndexService algoliaSearchService;

    public SimpleSearchService(IAlgoliaIndexService algoliaSearchService) => this.algoliaSearchService = algoliaSearchService;

    public async Task<SearchResponse<DancingGoatSimpleSearchResultModel>> GlobalSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {
        var index = await algoliaSearchService.InitializeIndex(indexName, default);

        page = Math.Max(page, 1);

        var query = new Query(searchText)
        {
            Page = page - 1,
            HitsPerPage = pageSize
        };
            
        var results = await index.SearchAsync<DancingGoatSimpleSearchResultModel>(query);
        return results;
    }
}
