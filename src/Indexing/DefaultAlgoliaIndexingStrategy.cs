﻿using Algolia.Search.Models.Settings;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Default indexing startegy just implements the methods but does not change the data.
/// </summary>
public class DefaultAlgoliaIndexingStrategy : IAlgoliaIndexingStrategy
{
    public virtual IndexSettings GetAlgoliaIndexSettings() => new IndexSettings();

    /// <inheritdoc />
    public virtual Task<IEnumerable<JObject>> MapToAlgoliaJObjectsOrNull(IIndexEventItemModel algoliaPageItem) => Task.FromResult<IEnumerable<JObject>>([]);

    public virtual async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventWebPageItemModel changedItem) => await Task.FromResult(new List<IndexEventWebPageItemModel>() { changedItem });

    public virtual async Task<IEnumerable<IIndexEventItemModel>> FindItemsToReindex(IndexEventReusableItemModel changedItem) => await Task.FromResult(new List<IndexEventWebPageItemModel>());
}

