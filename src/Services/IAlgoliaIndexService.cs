using System;

using Algolia.Search.Clients;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Initializes <see cref="ISearchIndex" /> instances.
    /// </summary>
    public interface IAlgoliaIndexService
    {
        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> for the given <paramref name="indexName" />
        /// and calls <see cref="ISearchIndex.SetSettings"/>.
        /// </summary>
        /// <param name="indexName">The code name of the index.</param>
        /// <exception cref="InvalidOperationException" />
        ISearchIndex InitializeIndex(string indexName);
    }
}