namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// A queued item to be processed by <see cref="AlgoliaCrawlerQueueWorker"/> which
    /// represents a change made to a page in the content tree.
    /// </summary>
    public sealed class AlgoliaCrawlerQueueItem
    {
        /// <summary>
        /// The ID of the crawler to update.
        /// </summary>
        public string CrawlerId
        {
            get;
        }


        /// <summary>
        /// The URL of the Xperience page.
        /// </summary>
        public string Url
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
        /// Initializes a new instance of the <see cref="AlgoliaCrawlerQueueItem"/> class.
        /// </summary>
        /// <param name="crawlerId">The ID of the crawler to update.</param>
        /// <param name="url">The URL of the Xperience page.</param>
        /// <param name="taskType">The type of the Algolia task.</param>
        public AlgoliaCrawlerQueueItem(string crawlerId, string url, AlgoliaTaskType taskType)
        {
            CrawlerId = crawlerId;
            Url = url;
            TaskType = taskType;
        }
    }
}
