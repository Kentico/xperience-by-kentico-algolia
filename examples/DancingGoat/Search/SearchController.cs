using DancingGoat.Search.Services;
using Microsoft.AspNetCore.Mvc;

namespace DancingGoat.Search;

[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly SimpleSearchService simpleSearchService;
    private readonly AdvancedSearchService advancedSearchService;

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
            var results = await advancedSearchService.GlobalSearch(indexName ?? "Advanced", query, page, pageSize, facet);
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
        var results = await simpleSearchService.GlobalSearch("Default", query, page, pageSize);

        return View(results);
    }
}
