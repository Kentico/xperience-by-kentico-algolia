using System;
using System.Collections.Generic;
using System.Linq;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Represents a store of Algolia indexes and crawlers.
    /// </summary>
    public sealed class IndexStore
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

            index.Identifier = registeredIndexes.Count + 1;
            registeredIndexes.Add(index);
        }

        public void AddIndices(IEnumerable<AlgoliaConfigurationModel> models)
        {
            registeredIndexes.Clear();
            foreach (var index in models)
            {
                Instance.AddIndex(new AlgoliaIndex(
                    index.IndexName,
                    index.ChannelName,
                    index.LanguageNames.ToList(),
                    index.Id,
                    index.Paths ?? new List<IncludedPath>(),
                    (IAlgoliaIndexingStrategy)Activator.CreateInstance(StrategyStorage.Strategies[index.StrategyName])
                ));
            }
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
        public IEnumerable<AlgoliaIndex> GetAllIndices()
        {
            return registeredIndexes;
        }



        internal void ClearIndexes()
        {
            registeredIndexes.Clear();
        }


        internal AlgoliaIndex GetIndex(int id)
        {
            return registeredIndexes.FirstOrDefault(i => i.Identifier == id);
        }
    }
}
