using CMS.DocumentEngine;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaQueueWorker"/> which
    /// represents a recent change made to an indexed <see cref="TreeNode"/>.
    /// </summary>
    public class AlgoliaQueueItem
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
        /// <c>true</c> if the <see cref="Node"/> data should be removed from the Algolia index.
        /// </summary>
        public bool Delete
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
    }
}