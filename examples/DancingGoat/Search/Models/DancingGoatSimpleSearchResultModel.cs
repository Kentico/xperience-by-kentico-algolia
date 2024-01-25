using Kentico.Xperience.Algolia.Search;

namespace DancingGoat.Search.Models;

public class DancingGoatSimpleSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; }
}
