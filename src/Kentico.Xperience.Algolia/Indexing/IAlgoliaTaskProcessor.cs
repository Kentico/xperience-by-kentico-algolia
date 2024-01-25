namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Processes tasks from <see cref="AlgoliaQueueWorker"/>.
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
}
