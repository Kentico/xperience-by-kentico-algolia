using System;
using System.Linq;

using CMS.Core;
using CMS.Websites.Internal;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Algolia extension methods for the <see cref="IndexEventWebPageItemModel"/> class.
/// </summary>
internal static class IndexedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in any registered Algolia index.
    /// </summary>
    /// <param name="indexedItem">The <see cref="IndexEventWebPageItemModel"/> to check for indexing.</param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsAlgoliaIndexed(this IndexEventWebPageItemModel indexedItem, IEventLogService log, string eventName)
    {
        if (indexedItem == null)
        {
            throw new ArgumentNullException(nameof(indexedItem));
        }

        foreach (var index in AlgoliaIndexStore.Instance.GetAllIndices())
        {
            if (indexedItem.IsIndexedByIndex(log, index.IndexName, eventName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAlgoliaIndexed(this IndexEventReusableItemModel indexedItem, IEventLogService log, string eventName)
    {
        if (indexedItem == null)
        {
            throw new ArgumentNullException(nameof(indexedItem));
        }

        foreach (var index in AlgoliaIndexStore.Instance.GetAllIndices())
        {
            if (indexedItem.IsIndexedByIndex(log, index.IndexName, eventName))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Returns true if the node is included in the Algolia index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="indexName">The Algolia index code name.</param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsIndexedByIndex(this IndexEventWebPageItemModel indexedItemModel, IEventLogService log, string indexName, string eventName)
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
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Lucene index '{indexName}' for event [{eventName}].");
            return false;
        }

        return algoliaIndex.IncludedPaths.Any(includedPathAttribute =>
        {
            bool matchesContentType = includedPathAttribute.ContentTypes == null || includedPathAttribute.ContentTypes.Count == 0 || includedPathAttribute.ContentTypes.Contains(indexedItemModel.ContentTypeName);
            if (includedPathAttribute.AliasPath.EndsWith("/"))
            {
                string pathToMatch = includedPathAttribute.AliasPath;
                var pathsOnPath = TreePathUtils.GetTreePathsOnPath(indexedItemModel.WebPageItemTreePath, true, false).ToHashSet();

                return pathsOnPath.Contains(pathToMatch) && matchesContentType;
            }
            else
            {
                if (indexedItemModel.WebPageItemTreePath is null)
                {
                    return false;
                }
                return indexedItemModel.WebPageItemTreePath.Equals(includedPathAttribute.AliasPath, StringComparison.OrdinalIgnoreCase) && matchesContentType;
            }
        });
    }

    /// <summary>
    /// Returns true if the node is included in the Algolia index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="indexName">The Algolia index code name.</param>
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
            log.LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Lucene index '{indexName}' for event [{eventName}].");
            return false;
        }

        if (algoliaIndex.LanguageNames.Any(x => x == indexedItemModel.LanguageName))
        {
            return true;
        }

        return false;
    }
}
