namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// The configuration of an Algolia crawler.
    /// </summary>
    /// <remarks>See <see href="https://www.algolia.com/doc/rest-api/crawler/#get-a-crawler"/>.</remarks>
    public class AlgoliaCrawler
    {
        /// <summary>
        /// The crawler ID.
        /// </summary>
        public string Id
        {
            get;
            set;
        }


        /// <summary>
        /// The crawler name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// The crawler configuration.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/tools/crawler/getting-started/quick-start/#default-configuration-file"/>.</remarks>
        public AlgoliaCrawlerConfig Config
        {
            get;
            set;
        }
    }
}
