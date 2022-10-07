using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Represents a store of Algolia indexes and crawlers.
    /// </summary>
    public sealed class IndexStore
    {
        private static readonly Lazy<IndexStore> mInstance = new();
        private readonly List<AlgoliaIndex> registeredIndexes = new();
        private readonly HashSet<string> registeredCrawlers = new();


        /// <summary>
        /// Gets current instance of the <see cref="IndexStore"/> class.
        /// </summary>
        public static IndexStore Instance => mInstance.Value;


        /// <summary>
        /// Adds an index to the store.
        /// </summary>
        /// <param name="index">The index to add.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public void AddIndex(AlgoliaIndex index)
        {
            if (index == null)
            {
                throw new ArgumentNullException(nameof(index));
            }

            if (registeredIndexes.Any(i => i.IndexName.Equals(index.IndexName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Attempted to register Algolia index with name '{index.IndexName},' but it is already registered.");
            }

            var facetableProperties = index.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, typeof(FacetableAttribute)));
            if (facetableProperties.Any(prop => {
                var attr = prop.GetCustomAttributes<FacetableAttribute>(false).FirstOrDefault();
                return attr.FilterOnly && attr.Searchable;
            }))
            {
                throw new InvalidOperationException($"Facetable attributes cannot be both {nameof(FacetableAttribute.Searchable)} and {nameof(FacetableAttribute.FilterOnly)}.");
            }

            AddIncludedPaths(index);

            index.Identifier = registeredIndexes.Count + 1;
            registeredIndexes.Add(index);
        }


        /// <summary>
        /// Adds a crawler to the store.
        /// </summary>
        /// <param name="crawlerId">The ID of the crawler to add.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public void AddCrawler(string crawlerId)
        {
            if (String.IsNullOrEmpty(crawlerId))
            {
                throw new ArgumentNullException(crawlerId);
            }

            if (registeredCrawlers.Any(id => id.Equals(crawlerId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Attempted to register Algolia crawler with ID '{crawlerId},' but it is already registered.");
            }

            registeredCrawlers.Add(crawlerId);
        }


        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="indexName"/>,
        /// or <c>null</c>.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public AlgoliaIndex GetIndex(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            return registeredIndexes.SingleOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Gets all registered indexes.
        /// </summary>
        public IEnumerable<AlgoliaIndex> GetAllIndexes()
        {
            return registeredIndexes;
        }


        /// <summary>
        /// Gets all registered crawlers.
        /// </summary>
        public IEnumerable<string> GetAllCrawlers()
        {
            return registeredCrawlers;
        }


        private static void AddIncludedPaths(AlgoliaIndex index)
        {
            var paths = index.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            foreach (var path in paths)
            {
                path.Identifier = Guid.NewGuid().ToString();
            }

            index.IncludedPaths = paths;
        }


        internal AlgoliaIndex GetIndex(int id)
        {
            return registeredIndexes.FirstOrDefault(i => i.Identifier == id);
        }
    }
}
