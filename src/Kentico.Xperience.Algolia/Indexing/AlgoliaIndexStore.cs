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
        /// Gets all registered indexes.
        /// </summary>
        public IEnumerable<AlgoliaIndex> GetAllIndices() => registeredIndexes;

        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="indexName"/>,
        /// or <c>null</c>.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public AlgoliaIndex? GetIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                return null;
            }

            return registeredIndexes.SingleOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="identifier"/>,
        /// or <c>null</c>.
        /// </summary>
        /// <param name="identifier">The identifier of the index to retrieve.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public AlgoliaIndex? GetIndex(int identifier) => registeredIndexes.Find(i => i.Identifier == identifier);

        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="indexName"/>. If no index is found, a <see cref="InvalidOperationException" /> is thrown.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public AlgoliaIndex GetRequiredIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentException("Value must not be null or empty");
            }

            return registeredIndexes.SingleOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"The index '{indexName}' is not registered.");
        }

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

            if (registeredIndexes.Exists(i => i.IndexName.Equals(index.IndexName, StringComparison.OrdinalIgnoreCase) || index.Identifier == i.Identifier))
            {
                throw new InvalidOperationException($"Attempted to register Algolia index with identifer [{index.Identifier}] and name [{index.IndexName}] but it is already registered.");
            }

            registeredIndexes.Add(index);
        }

        /// <summary>
        /// Resets all indicies
        /// </summary>
        /// <param name="models"></param>
        internal void SetIndicies(IEnumerable<AlgoliaConfigurationModel> models)
        {
            registeredIndexes.Clear();
            foreach (var index in models)
            {
                Instance.AddIndex(new AlgoliaIndex(index, StrategyStorage.Strategies));
            }
        }
    }
}
