using Algolia.Search.Models.Settings;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Indexing;

public interface IAlgoliaIndexingStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data.
    /// </summary>
    /// <param name="algoliaPageItem">The <see cref="IIndexEventItemModel"/> currently being indexed.</param>
    /// <returns>Modified Algolia document.</returns>
    Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem);

    IndexSettings GetAlgoliaIndexSettings();

    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem);

    Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem);
}
