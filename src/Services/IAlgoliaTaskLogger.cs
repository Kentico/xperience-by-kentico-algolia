using CMS.DocumentEngine;

using Kentico.Xperience.AlgoliaSearch.Models;

using System.Collections.Generic;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    public interface IAlgoliaTaskLogger
    {
        void LogTask(AlgoliaQueueItem task);


        void LogTasks(IEnumerable<AlgoliaQueueItem> tasks);


        /// <summary>
        /// Loops through all registered Algolia indexes and logs a task if the passed
        /// <paramref name="node"/> is indexed. For updated pages, a task is only logged
        /// if one of the indexed columns has been modified.
        /// </summary>
        /// <remarks>Logs an error if there are issues loading indexed columns.</remarks>
        /// <param name="node">The <see cref="TreeNode"/> that triggered the event.</param>
        /// <param name="eventName">The name of the Xperience event that was triggered.</param>
        /// <returns>The number of tasks that were created.</returns>
        void HandleEvent(TreeNode node, string eventName);
    }
}
