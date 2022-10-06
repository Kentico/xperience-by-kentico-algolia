using System;
using System.Collections.Generic;
using System.Linq;

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
        }


        /// <summary>
        /// The type of the Algolia task.
        /// </summary>
        public AlgoliaTaskType TaskType
        {
            get;
        }


        /// <summary>
        /// The code name of the Algolia index to be updated.
        /// </summary>
        public string IndexName
        {
            get;
        }


        /// <summary>
        /// The columns of the page that should be updated in Algolia. Only used when
        /// processing <see cref="AlgoliaTaskType.UPDATE"/> tasks.
        /// </summary>
        public IEnumerable<string> ChangedColumns
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaQueueItem"/> class.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that was changed.</param>
        /// <param name="taskType">The type of the Algolia task.</param>
        /// <param name="indexName">The code name of the Algolia index to be updated.</param>
        /// <param name="changedColumns">The columns of the page that should be updated in Algolia.
        /// Only used when processing <see cref="AlgoliaTaskType.UPDATE"/> tasks.</param>
        /// <exception cref="ArgumentNullException" />
        public AlgoliaQueueItem(TreeNode node, AlgoliaTaskType taskType, string indexName, IEnumerable<string> changedColumns = null)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (taskType == AlgoliaTaskType.UPDATE && (changedColumns == null || !changedColumns.Any()))
            {
                throw new InvalidOperationException("Changed columns are required for UPDATE tasks.");
            }

            Node = node ?? throw new ArgumentNullException(nameof(node));
            TaskType = taskType;
            IndexName = indexName;
            ChangedColumns = changedColumns;
        }
    }
}