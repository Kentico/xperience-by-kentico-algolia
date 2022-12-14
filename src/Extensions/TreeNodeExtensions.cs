using System;
using System.Linq;

using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Attributes;

namespace Kentico.Xperience.Algolia.Extensions
{
    /// <summary>
    /// Algolia extension methods for the <see cref="TreeNode"/> class.
    /// </summary>
    internal static class TreeNodeExtensions
    {
        /// <summary>
        /// Returns true if the node is included in any registered Algolia index.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> to check for indexing.</param>
        /// <exception cref="ArgumentNullException" />
        public static bool IsAlgoliaIndexed(this TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return IndexStore.Instance.GetAllIndexes().Any(index => node.IsIndexedByIndex(index.IndexName));
        }


        /// <summary>
        /// Returns true if the node is included in the Algolia index's allowed
        /// paths as set by the <see cref="IncludedPathAttribute"/>.
        /// </summary>
        /// <remarks>Logs an error if the search model cannot be found.</remarks>
        /// <param name="node">The node to check for indexing.</param>
        /// <param name="indexName">The Algolia index code name.</param>
        /// <exception cref="ArgumentNullException" />
        public static bool IsIndexedByIndex(this TreeNode node, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                Service.Resolve<IEventLogService>().LogError(nameof(TreeNodeExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
                return false;
            }

            return algoliaIndex.IncludedPaths.Any(includedPathAttribute => {
                var matchesContentType = includedPathAttribute.ContentTypes.Length == 0 || includedPathAttribute.ContentTypes.Contains(node.ClassName);
                if (includedPathAttribute.AliasPath.EndsWith("/%"))
                {
                    var pathToMatch = TreePathUtils.EnsureSingleNodePath(includedPathAttribute.AliasPath);
                    var pathsOnPath = TreePathUtils.GetNodeAliasPathsOnPath(node.NodeAliasPath, true, false).ToHashSet();

                    return pathsOnPath.Contains(pathToMatch) && matchesContentType;
                }
                else
                {
                    return node.NodeAliasPath.Equals(includedPathAttribute.AliasPath, StringComparison.OrdinalIgnoreCase) && matchesContentType;
                }
            });
        }
    }
}
