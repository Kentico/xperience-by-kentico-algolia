using System;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;

using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Initializes <see cref="ISearchIndex" /> instances.
    /// </summary>
    public interface IAlgoliaIndexService
    {
        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> to manage the underlying Algolia index for
        /// the provided <paramref name="crawler"/>.
        /// </summary>
        /// <param name="crawler">The crawler definition as retrieved by <see cref="IAlgoliaClient.GetCrawler"/>.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        ISearchIndex InitializeCrawler(AlgoliaCrawler crawler);


        /// <summary>
        /// Initializes a new <see cref="ISearchIndex" /> for the given <paramref name="indexName" />
        /// and calls <see cref="ISearchIndex.SetSettings"/>.
        /// </summary>
        /// <param name="indexName">The code name of the index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="InvalidOperationException" />
        Task<ISearchIndex> InitializeIndex(string indexName, CancellationToken cancellationToken);
    }
}