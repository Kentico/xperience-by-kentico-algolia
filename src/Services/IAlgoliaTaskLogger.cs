using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Contains methods for logging <see cref="AlgoliaQueueItem"/>s and <see cref="AlgoliaCrawlerQueueItem"/>s
    /// for processing by <see cref="AlgoliaQueueWorker"/> and <see cref="AlgoliaCrawlerQueueWorker"/>.
    /// </summary>
    public interface IAlgoliaTaskLogger
    {
        /// <summary>
        /// Logs a <see cref="AlgoliaCrawlerQueueItem"/> for each registered crawler. Then, loops
        /// through all registered Algolia indexes and logs a task if the passed <paramref name="node"/> is indexed.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that triggered the event.</param>
        /// <param name="eventName">The name of the Xperience event that was triggered.</param>
        void HandleEvent(TreeNode node, string eventName);
    }
}
