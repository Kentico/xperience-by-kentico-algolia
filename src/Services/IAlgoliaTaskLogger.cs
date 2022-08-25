using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Models;

using System.Collections.Generic;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Contains methods for logging <see cref="AlgoliaQueueItem"/>s for processing by
    /// <see cref="AlgoliaQueueWorker"/>.
    /// </summary>
    public interface IAlgoliaTaskLogger
    {
        /// <summary>
        /// Logs a single <see cref="AlgoliaQueueItem"/>.
        /// </summary>
        /// <param name="task">The task to log.</param>
        void LogTask(AlgoliaQueueItem task);


        /// <summary>
        /// Logs multiple <see cref="AlgoliaQueueItem"/>s.
        /// </summary>
        /// <param name="tasks">The tasks to log.</param>
        void LogTasks(IEnumerable<AlgoliaQueueItem> tasks);


        /// <summary>
        /// Loops through all registered Algolia indexes and logs a task if the passed
        /// <paramref name="node"/> is indexed.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that triggered the event.</param>
        /// <param name="eventName">The name of the Xperience event that was triggered.</param>
        void HandleEvent(TreeNode node, string eventName);
    }
}
