using Algolia.Search.Clients;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Initializes <see cref="ISearchIndex" /> instances.
    /// </summary>
    public interface IAlgoliaIndexService
    {
        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> for the given <paramref name="indexName" />.
        /// </summary>
        /// <param name="indexName">The code name of the index.</param>
        ISearchIndex InitializeIndex(string indexName);
    }
}