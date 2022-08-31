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
            set;
        }


        /// <summary>
        /// The type of the Algolia task.
        /// </summary>
        public AlgoliaTaskType TaskType
        {
            get;
            set;
        }


        /// <summary>
        /// The code name of the Algolia index to be updated.
        /// </summary>
        public string IndexName
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the type of the <see cref="AlgoliaQueueItem"/>.
        /// </summary>
        public enum AlgoliaTaskType
        {
            /// <summary>
            /// Unsupported task type.
            /// </summary>
            UNKNOWN,

            /// <summary>
            /// A task for a page which was published for the first time.
            /// </summary>
            CREATE,

            /// <summary>
            /// A task for a page which was previously published.
            /// </summary>
            UPDATE,

            /// <summary>
            /// A task for a page which should be removed from the index.
            /// </summary>
            DELETE
        }
    }
}