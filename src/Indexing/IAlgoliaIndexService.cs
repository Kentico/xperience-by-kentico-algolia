using System;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// Initializes <see cref="ISearchIndex" /> instances.
    /// </summary>
    public interface IAlgoliaIndexService
    {
        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> for the given <paramref name="indexName" />
        /// </summary>
        /// <param name="indexName">The code name of the index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="InvalidOperationException" />
        Task<ISearchIndex> InitializeIndex(string indexName, CancellationToken cancellationToken);
    }
}