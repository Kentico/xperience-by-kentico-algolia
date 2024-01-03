using Algolia.Search.Models.Settings;
using Kentico.Xperience.Algolia.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Services;

public interface IAlgoliaIndexingStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data.
    /// </summary>
    /// <param name="algoliaPageItem">The <see cref="IndexedItemModel"/> currently being indexed.</param>
    /// <returns>Modified Algolia document.</returns>
    Task<IEnumerable<JObject>?> MapToAlgoliaJObjecstOrNull(IndexedItemModel algoliaPageItem);

    IndexSettings GetAlgoliaIndexSettings();

    Task<IEnumerable<IndexedItemModel>> FindItemsToReindex(IndexedItemModel changedItem);

    Task<IEnumerable<IndexedItemModel>> FindItemsToReindex(IndexedContentItemModel changedItem, string currentlyIndexedWebsiteChannelName);
}
