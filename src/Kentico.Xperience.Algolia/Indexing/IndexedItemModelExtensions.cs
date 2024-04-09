using CMS.Core;
using CMS.Websites.Internal;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Algolia extension methods for the <see cref="IndexEventWebPageItemModel"/> class.
/// </summary>
internal static class IndexedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in the Algolia index based on the index's defined paths
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The Algolia index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventWebPageItemModel indexedItemModel, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (indexedItemModel is null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var algoliaIndex = AlgoliaIndexStore.Instance.GetIndex(indexName);
        if (algoliaIndex is null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}' for event [{eventName}].");
            return false;
        }

        if (!algoliaIndex.LanguageNames.Exists(x => x == indexedItemModel.LanguageName))
        {
            return false;
        }

        return algoliaIndex.IncludedPaths.Any(path =>
        {
            bool matchesContentType = path.ContentTypes.Exists(x => string.Equals(x.ContentTypeName, indexedItemModel.ContentTypeName));

            if (!matchesContentType)
            {
                return false;
            }

            // Supports wildcard matching
            if (path.AliasPath.EndsWith("/%", StringComparison.OrdinalIgnoreCase))
            {
                string pathToMatch = path.AliasPath[..^2];
                var pathsOnPath = TreePathUtils.GetTreePathsOnPath(indexedItemModel.WebPageItemTreePath, true, false).ToHashSet();

                return pathsOnPath.Any(p => p.StartsWith(pathToMatch, StringComparison.OrdinalIgnoreCase));
            }

            return indexedItemModel.WebPageItemTreePath.Equals(path.AliasPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Returns true if the node is included in the Algolia index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="indexName">The Algolia index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventReusableItemModel indexedItemModel, IEventLogService log, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (indexedItemModel == null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var algoliaIndex = AlgoliaIndexStore.Instance.GetIndex(indexName);
        if (algoliaIndex == null)
        {
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}' for event [{eventName}].");
            return false;
        }

        if (algoliaIndex.LanguageNames.Exists(x => x == indexedItemModel.LanguageName))
        {
            return true;
        }

        return false;
    }
}
