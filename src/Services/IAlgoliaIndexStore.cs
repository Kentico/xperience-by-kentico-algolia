using Kentico.Xperience.AlgoliaSearch.Models;

using System.Collections.Generic;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Contains a collection of registered <see cref="AlgoliaIndex"/>es.
    /// </summary>
    public interface IAlgoliaIndexStore
    {
        /// <summary>
        /// Inserts an <see cref="AlgoliaIndex"/> into the store.
        /// </summary>
        /// <typeparam name="TModel">The search model class.</typeparam>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <returns>The <see cref="IAlgoliaIndexStore"/> for chaining.</returns>
        IAlgoliaIndexStore Add<TModel>(string indexName) where TModel : AlgoliaSearchModel;


        /// <summary>
        /// Gets all registered Algolia indexes.
        /// </summary>
        IEnumerable<AlgoliaIndex> GetAllIndexes();


        /// <summary>
        /// Gets a registered <see cref="AlgoliaIndex"/> with the specified <paramref name="indexName"/>,
        /// or <c>null</c>.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve.</param>
        AlgoliaIndex GetIndex(string indexName);
    }
}
