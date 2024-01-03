using Kentico.Xperience.Algolia.Models;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Contains methods for logging <see cref="AlgoliaQueueItem"/>s and <see cref="AlgoliaCrawlerQueueItem"/>s
    /// for processing by <see cref="AlgoliaQueueWorker"/> and <see cref="AlgoliaCrawlerQueueWorker"/>.
    /// </summary>
    public interface IAlgoliaTaskLogger
    {
        /// <summary>
        /// Logs an <see cref="AlgoliaCrawlerQueueItem"/> for each registered crawler. Then, loops
        /// through all registered Algolia indexes and logs a task if the passed <paramref name="node"/> is indexed.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that triggered the event.</param>
        /// <param name="eventName">The name of the Xperience event that was triggered.</param>
        Task HandleEvent(IndexedItemModel indexedModel, string eventName);

        Task HandleContentItemEvent(IndexedContentItemModel indexedItem, string eventName);
    }
}
