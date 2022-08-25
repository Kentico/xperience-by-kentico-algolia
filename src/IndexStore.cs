using Kentico.Xperience.AlgoliaSearch.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Xperience.AlgoliaSearch
{
    /// <summary>
    /// Represents a store of Algolia indexes.
    /// </summary>
    public class IndexStore
    {
        private static readonly Lazy<IndexStore> mInstance = new();
        private readonly List<AlgoliaIndex> registeredIndexes = new();


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
        public IndexStore Add(AlgoliaIndex index)
        {
            if (index == null)
            {
                throw new InvalidOperationException(nameof(index));
            }

            if (registeredIndexes.Any(i => i.IndexName.Equals(index.IndexName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Attempted to register Algolia index with name '{index.IndexName},' but it is already registered.");
            }

            registeredIndexes.Add(index);

            return this;
        }


        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="indexName"/>,
        /// or <c>null</c>.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve.</param>
        public AlgoliaIndex Get(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            return registeredIndexes.FirstOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Gets all registered indexes.
        /// </summary>
        public IEnumerable<AlgoliaIndex> GetAll()
        {
            return registeredIndexes;
        }
    }
}
