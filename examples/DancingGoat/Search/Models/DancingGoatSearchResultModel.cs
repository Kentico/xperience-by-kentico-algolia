using Kentico.Xperience.Algolia.Search;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; } = string.Empty;
    public string SortableTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
