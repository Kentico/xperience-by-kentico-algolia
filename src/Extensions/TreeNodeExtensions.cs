using CMS.Core;
using CMS.DocumentEngine;

using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Linq;
using System.Reflection;

namespace Kentico.Xperience.AlgoliaSearch.Extensions
{
    public static class TreeNodeExtensions
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

            return Service.Resolve<IAlgoliaIndexStore>().GetAllIndexes().Any(index => node.IsIndexedByIndex(index.IndexName));
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

            var alogliaIndex = Service.Resolve<IAlgoliaIndexStore>().GetIndex(indexName);
            if (alogliaIndex == null)
            {
                Service.Resolve<IEventLogService>().LogError(nameof(TreeNodeExtensions), nameof(IsIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
                return false;
            }

            var includedPathAttributes = alogliaIndex.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            return includedPathAttributes.Any(includedPathAttribute => {
                var matchesPageType = (includedPathAttribute.PageTypes.Length == 0 || includedPathAttribute.PageTypes.Contains(node.ClassName));
                if (includedPathAttribute.AliasPath.EndsWith("/%"))
                {
                    var pathToMatch = TreePathUtils.EnsureSingleNodePath(includedPathAttribute.AliasPath);
                    var pathsOnPath = TreePathUtils.GetNodeAliasPathsOnPath(node.NodeAliasPath, false, false);
                    var matchesPath = pathsOnPath.Any(path => path.Equals(pathToMatch, StringComparison.OrdinalIgnoreCase));

                    return matchesPath && matchesPageType;
                }
                else
                {
                    return node.NodeAliasPath.Equals(includedPathAttribute.AliasPath, StringComparison.OrdinalIgnoreCase) && matchesPageType;
                }
            });
        }
    }
}
