using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// Contains methods for logging <see cref="AlgoliaQueueItem"/>s and <see cref="AlgoliaQueueItem"/>s
    /// for processing by <see cref="AlgoliaQueueWorker"/> and <see cref="AlgoliaQueueWorker"/>.
    /// </summary>
    public interface IAlgoliaTaskLogger
    {
        /// <summary>
        /// Logs an <see cref="AlgoliaQueueItem"/> for each registered crawler. Then, loops
        /// through all registered Algolia indexes and logs a task if the passed <paramref name="webpageItem"/> is indexed.
        /// </summary>
        /// <param name="webpageItem">The <see cref="IndexEventWebPageItemModel"/> that triggered the event.</param>
        /// <param name="eventName">The name of the Xperience event that was triggered.</param>
        Task HandleEvent(IndexEventWebPageItemModel webpageItem, string eventName);

        Task HandleReusableItemEvent(IndexEventReusableItemModel reusableItem, string eventName);
    }
}
