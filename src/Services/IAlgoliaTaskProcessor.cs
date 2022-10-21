using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Processes tasks from <see cref="AlgoliaQueueWorker"/> and <see cref="AlgoliaCrawlerQueueWorker"/>.
    /// </summary>
    public interface IAlgoliaTaskProcessor
    {
        /// <summary>
        /// Processes multiple queue items from all Algolia indexes in batches. Algolia
        /// automatically applies batching in multiples of 1,000 when using their API,
        /// so all queue items are forwarded to the API.
        /// </summary>
        /// <param name="queueItems">The items to process.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>The number of items processed.</returns>
        Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> queueItems, CancellationToken cancellationToken);


        /// <summary>
        /// Processes multiple queue items from all Algolia crawlers.
        /// </summary>
        /// <param name="queueItems">The items to process.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>The number of items processed.</returns>
        Task<int> ProcessCrawlerTasks(IEnumerable<AlgoliaCrawlerQueueItem> queueItems, CancellationToken cancellationToken);
    }
}
