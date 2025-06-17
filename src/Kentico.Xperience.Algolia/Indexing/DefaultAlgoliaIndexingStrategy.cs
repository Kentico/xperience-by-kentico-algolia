using Algolia.Search.Models.Settings;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Default indexing startegy just implements the methods but does not change the data.
/// </summary>
public class DefaultAlgoliaIndexingStrategy : IAlgoliaIndexingStrategy
{
    public virtual IndexSettings GetAlgoliaIndexSettings() => new();

    /// <inheritdoc />
    public virtual Task<IEnumerable<JObject>?> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem)
    {
        if (algoliaPageItem.IsSecured)
        {
            return Task.FromResult<IEnumerable<JObject>?>(null);
        }

        var jObject = new JObject
        {
            [nameof(IIndexEventItemModel.Name)] = algoliaPageItem.Name
        };

        var result = new List<JObject>()
        {
            jObject
        };

        return Task.FromResult<IEnumerable<JObject>?>(result);
    }

    public virtual async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem) => await Task.FromResult(new List<IndexEventWebPageItemModel>() { changedItem });

    public virtual async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem) => await Task.FromResult(new List<IndexEventWebPageItemModel>());
}

