using System;
using System.Linq;

using CMS.Core;
using CMS.Websites.Internal;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Extensions;

/// <summary>
/// Algolia extension methods for the <see cref="IndexedItemModel"/> class.
/// </summary>
internal static class IndexedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in any registered Algolia index.
    /// </summary>
    /// <param name="indexedItem">The <see cref="IndexedItemModel"/> to check for indexing.</param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsAlgoliaIndexed(this IndexedItemModel indexedItem, string eventName)
    {
        if (indexedItem == null)
        {
            throw new ArgumentNullException(nameof(indexedItem));
        }

        foreach (var index in IndexStore.Instance.GetAllIndices())
        {
            if (indexedItem.IsIndexedByIndex(index.IndexName, eventName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAlgoliaIndexed(this IndexedContentItemModel indexedItem, string eventName)
    {
        if (indexedItem == null)
        {
            throw new ArgumentNullException(nameof(indexedItem));
        }

        foreach (var index in IndexStore.Instance.GetAllIndices())
        {
            if (indexedItem.IsIndexedByIndex(index.IndexName, eventName))
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
    public static bool IsIndexedByIndex(this IndexedItemModel indexedItemModel, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (indexedItemModel == null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
        if (algoliaIndex == null)
        {
            Service.Resolve<IEventLogService>().LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
            return false;
        }

        return algoliaIndex.IncludedPaths.Any(includedPathAttribute =>
        {
            bool matchesContentType = includedPathAttribute.ContentTypes == null || includedPathAttribute.ContentTypes.Length == 0 || includedPathAttribute.ContentTypes.Contains(indexedItemModel.ClassName);
            if (includedPathAttribute.AliasPath.EndsWith("/"))
            {
                string? pathToMatch = includedPathAttribute.AliasPath;
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
    public static bool IsIndexedByIndex(this IndexedContentItemModel indexedItemModel, string indexName, string eventName)
    {
        if (string.IsNullOrEmpty(indexName))
        {
            throw new ArgumentNullException(nameof(indexName));
        }
        if (indexedItemModel == null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
        if (algoliaIndex == null)
        {
            Service.Resolve<IEventLogService>().LogError(nameof(IndexedItemModelExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
            return false;
        }

        if (algoliaIndex.LanguageCodes.Any(x => x == indexedItemModel.LanguageCode))
        {
            return true;
        }

        return false;
    }
}
