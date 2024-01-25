using Kentico.Xperience.Algolia.Search;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
