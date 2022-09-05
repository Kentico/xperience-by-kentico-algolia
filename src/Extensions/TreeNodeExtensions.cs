using System;
using System.Linq;
using System.Reflection;

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

            return IndexStore.Instance.GetAll().Any(index => node.IsIndexedByIndex(index.IndexName));
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

            var algoliaIndex = IndexStore.Instance.Get(indexName);
            if (algoliaIndex == null)
            {
                Service.Resolve<IEventLogService>().LogError(nameof(TreeNodeExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
                return false;
            }

            var includedPathAttributes = algoliaIndex.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            return includedPathAttributes.Any(includedPathAttribute => {
                var matchesPageType = (includedPathAttribute.PageTypes.Length == 0 || includedPathAttribute.PageTypes.Contains(node.ClassName));
                if (includedPathAttribute.AliasPath.EndsWith("/%"))
                {
                    var pathToMatch = TreePathUtils.EnsureSingleNodePath(includedPathAttribute.AliasPath);
                    var pathsOnPath = TreePathUtils.GetNodeAliasPathsOnPath(node.NodeAliasPath, false, false).ToHashSet();

                    return pathsOnPath.Contains(pathToMatch) && matchesPageType;
                }
                else
                {
                    return node.NodeAliasPath.Equals(includedPathAttribute.AliasPath, StringComparison.OrdinalIgnoreCase) && matchesPageType;
                }
            });
        }
    }
}
