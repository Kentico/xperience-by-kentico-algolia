using Algolia.Search.Models.Settings;
using Kentico.Xperience.Algolia.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Services;

/// <summary>
/// Default indexing startegy just implements the methods but does not change the data.
/// </summary>
public class DefaultAlgoliaIndexingStrategy : IAlgoliaIndexingStrategy
{
    public virtual IndexSettings GetAlgoliaIndexSettings() => new IndexSettings();

    /// <inheritdoc />
    public virtual Task<IEnumerable<JObject>?> MapToAlgoliaJObjecstOrNull(IndexedItemModel algoliaPageItem) => Task.FromResult<IEnumerable<JObject?>>([]);

    public virtual async Task<IEnumerable<IndexedItemModel>> FindItemsToReindex(IndexedItemModel changedItem) => await Task.FromResult(new List<IndexedItemModel>() { changedItem });

    public virtual async Task<IEnumerable<IndexedItemModel>> FindItemsToReindex(IndexedContentItemModel changedItem, string currentlyIndexedWebsiteChannelName) => await Task.FromResult(new List<IndexedItemModel>());
}

