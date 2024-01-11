using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Xperience.Algolia.Admin;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// Represents a store of Algolia indexes and crawlers.
    /// </summary>
    public sealed class AlgoliaIndexStore
    {
        private static readonly Lazy<AlgoliaIndexStore> mInstance = new();
        private readonly List<AlgoliaIndex> registeredIndexes = new();


        /// <summary>
        /// Gets current instance of the <see cref="AlgoliaIndexStore"/> class.
        /// </summary>
        public static AlgoliaIndexStore Instance => mInstance.Value;


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
                    index.Paths ?? new List<AlgoliaIndexIncludedPath>(),
                    StrategyStorage.Strategies[index.StrategyName] ?? typeof(DefaultAlgoliaIndexingStrategy)
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
            if (string.IsNullOrEmpty(indexName))
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
