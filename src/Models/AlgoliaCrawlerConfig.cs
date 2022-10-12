namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// An <see cref="AlgoliaCrawler"/>'s configuration.
    /// </summary>
    /// <remarks>See <see href="https://www.algolia.com/doc/tools/crawler/getting-started/quick-start/#default-configuration-file"/>.</remarks>
    public sealed class AlgoliaCrawlerConfig
    {
        /// <summary>
        /// A string prepended to all index names created by this crawler.
        /// </summary>
        public string IndexPrefix
        {
            get;
            set;
        }
    }
}
