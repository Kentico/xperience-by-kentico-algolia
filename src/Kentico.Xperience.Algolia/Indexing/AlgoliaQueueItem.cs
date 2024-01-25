using System;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaQueueWorker"/> which
    /// represents a recent change made to an indexed <see cref="ItemToIndex"/> which is a representation of a WebPageItem.
    /// </summary>
    public sealed class AlgoliaQueueItem
    {
        /// <summary>
        /// The <see cref="ItemToIndex"/> that was changed.
        /// </summary>
        public IIndexEventItemModel ItemToIndex
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
        /// <param name="itemToIndex">The <see cref="IIndexEventItemModel"/> that was changed.</param>
        /// <param name="taskType">The type of the Algolia task.</param>
        /// <param name="indexName">The code name of the Algolia index to be updated.</param>
        /// <exception cref="ArgumentNullException" />
        public AlgoliaQueueItem(IIndexEventItemModel itemToIndex, AlgoliaTaskType taskType, string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            ItemToIndex = itemToIndex;
            if (taskType != AlgoliaTaskType.PUBLISH_INDEX && itemToIndex == null)
            {
                throw new ArgumentNullException(nameof(itemToIndex));
            }
            TaskType = taskType;
            IndexName = indexName;
        }
    }
}