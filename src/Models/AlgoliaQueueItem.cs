using System;

using CMS.DocumentEngine;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaQueueWorker"/> which
    /// represents a recent change made to an indexed <see cref="TreeNode"/>.
    /// </summary>
    public sealed class AlgoliaQueueItem
    {
        /// <summary>
        /// The <see cref="TreeNode"/> that was changed.
        /// </summary>
        public TreeNode Node
        {
            get;
            private set;
        }


        /// <summary>
        /// The type of the Algolia task.
        /// </summary>
        public AlgoliaTaskType TaskType
        {
            get;
            private set;
        }


        /// <summary>
        /// The code name of the Algolia index to be updated.
        /// </summary>
        public string IndexName
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaQueueItem"/> class.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that was changed.</param>
        /// <param name="taskType">The type of the Algolia task.</param>
        /// <param name="indexName">The code name of the Algolia index to be updated.</param>
        /// <exception cref="ArgumentNullException" />
        public AlgoliaQueueItem(TreeNode node, AlgoliaTaskType taskType, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            Node = node ?? throw new ArgumentNullException(nameof(node));
            TaskType = taskType;
            IndexName = indexName;
        }
    }
}