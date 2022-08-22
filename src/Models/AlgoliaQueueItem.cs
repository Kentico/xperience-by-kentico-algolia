using CMS.DocumentEngine;

namespace Kentico.Xperience.AlgoliaSearch.Models
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaQueueWorker"/> which
    /// represents a recent change made to an indexed <see cref="TreeNode"/>.
    /// </summary>
    public class AlgoliaQueueItem
    {
        /// <summary>
        /// The <see cref="TreeNode"/> that was recently created, updated, or deleted.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// True if the <see cref="Node"/> was recently deleted and should be removed
        /// from the Algolia index.
        /// </summary>
        public bool Deleted
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
