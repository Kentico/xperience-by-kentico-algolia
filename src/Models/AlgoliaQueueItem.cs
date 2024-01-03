using System;
using System.Collections.Generic;
using System.Linq;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaQueueWorker"/> which
    /// represents a recent change made to an indexed <see cref="IndexedItemModel"/> which is a representation of a WebPageItem.
    /// </summary>
    public sealed class AlgoliaQueueItem
    {
        /// <summary>
        /// The <see cref="IndexedItemModel"/> that was changed.
        /// </summary>
        public IndexedItemModel IndexedItemModel
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
        /// Initializes a new instance of the <see cref="AlgoliaQueueItem"/> class.
        /// </summary>
        /// <param name="indexedItem">The <see cref="Models.IndexedItemModel"/> that was changed.</param>
        /// <param name="taskType">The type of the Algolia task.</param>
        /// <param name="indexName">The code name of the Algolia index to be updated.</param>
        /// Only used when processing <see cref="AlgoliaTaskType.UPDATE"/> tasks.</param>
        /// <exception cref="ArgumentNullException" />
        public AlgoliaQueueItem(IndexedItemModel indexedItem, AlgoliaTaskType taskType, string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            IndexedItemModel = indexedItem;
            if (taskType != AlgoliaTaskType.PUBLISH_INDEX && indexedItem == null)
            {
                throw new ArgumentNullException(nameof(indexedItem));
            }
            TaskType = taskType;
            IndexName = indexName;
        }
    }
}