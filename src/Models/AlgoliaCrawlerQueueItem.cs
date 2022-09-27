namespace Kentico.Xperience.Algolia.Models
{
    internal class AlgoliaCrawlerQueueItem
    {
        public string CrawlerId
        {
            get;
        }


        public string Url
        {
            get;
        }


        public AlgoliaTaskType TaskType
        {
            get;
        }


        public AlgoliaCrawlerQueueItem(string crawlerId, string url, AlgoliaTaskType taskType)
        {
            CrawlerId = crawlerId;
            Url = url;
            TaskType = taskType;
        }
    }
}
