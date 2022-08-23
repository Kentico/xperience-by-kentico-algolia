using Kentico.Xperience.AlgoliaSearch.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexStore"/>.
    /// </summary>
    public class DefaultAlgoliaIndexStore : IAlgoliaIndexStore
    {
        private readonly List<AlgoliaIndex> registeredIndexes = new List<AlgoliaIndex>();
        

        public IAlgoliaIndexStore Add<TModel>(string indexName) where TModel : AlgoliaSearchModel
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (registeredIndexes.Any(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Attempted to register Algolia index with name '{indexName},' but it is already registered.");
            }

            registeredIndexes.Add(new AlgoliaIndex
            {
                IndexName = indexName,
                Type = typeof(TModel)
            });

            return this;
        }


        public IEnumerable<AlgoliaIndex> GetAllIndexes()
        {
            return registeredIndexes;
        }


        public AlgoliaIndex GetIndex(string indexName)
        {
            return registeredIndexes.FirstOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
