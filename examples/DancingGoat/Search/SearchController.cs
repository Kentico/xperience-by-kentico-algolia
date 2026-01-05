using DancingGoat.Search.Services;

using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Search;

[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly SimpleSearchService simpleSearchService;
    private readonly AdvancedSearchService advancedSearchService;

    private const string NAME_OF_DEFAULT_INDEX = "Default";

    public SearchController(
        SimpleSearchService simpleSearchService,
        AdvancedSearchService advancedSearchService
        )
    {
        this.simpleSearchService = simpleSearchService;
        this.advancedSearchService = advancedSearchService;
    }

    public async Task<IActionResult> Index(string query, int pageSize = 10, int page = 1, string facet = null, string indexName = null)
    {
        try
        {
            var results = await advancedSearchService.GlobalSearch(indexName ?? NAME_OF_DEFAULT_INDEX, query, page, pageSize, facet);
            return View(results);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpGet("Simple")]
    public async Task<IActionResult> Simple(string query, int pageSize = 10, int page = 1)
    {
        var results = await simpleSearchService.GlobalSearch(NAME_OF_DEFAULT_INDEX, query, page, pageSize);

        return View(results);
    }
}
