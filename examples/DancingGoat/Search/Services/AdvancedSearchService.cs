using Algolia.Search.Models.Search;
using DancingGoat.Search.Models;
using Kentico.Xperience.Algolia.Indexing;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace DancingGoat.Search.Services;

public class AdvancedSearchService
{
    private readonly IAlgoliaIndexService algoliaSearchService;

    public AdvancedSearchService(IAlgoliaIndexService algoliaSearchService) => this.algoliaSearchService = algoliaSearchService;

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
