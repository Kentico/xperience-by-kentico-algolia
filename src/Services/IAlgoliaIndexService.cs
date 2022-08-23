using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;

using System;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Initializes <see cref="ISearchIndex" /> instances.
    /// </summary>
    public interface IAlgoliaIndexService
    {
        /// <summary>
        /// Gets the <see cref="IndexSettings"/> of the Algolia index.
        /// </summary>
        /// <param name="searchModel">The index search model class.</param>
        /// <returns>The index settings.</returns>
        /// <exception cref="ArgumentNullException" />
        IndexSettings GetIndexSettings(Type searchModel);


        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> for the given <paramref name="indexName" />
        /// and calls <see cref="ISearchIndex.SetSettings"/>.
        /// </summary>
        /// <param name="indexName">The code name of the index.</param>
        /// <exception cref="InvalidOperationException" />
        ISearchIndex InitializeIndex(string indexName);
    }
}