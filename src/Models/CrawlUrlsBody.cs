using System.Collections.Generic;
using Kentico.Xperience.Algolia.Services;
using Newtonsoft.Json;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Represents the JSON body of a <see cref="IAlgoliaClient.CrawlUrls"/> request.
    /// </summary>
    internal class CrawlUrlsBody
    {
        /// <summary>
        /// The URLs to crawl.
        /// </summary>
        [JsonProperty("urls")]
        public IEnumerable<string> Urls
        {
            get;
        }


        /// <summary>
        /// <para>When true, the given URLs are added to the extraUrls list of your configuration (unless already present in startUrls or sitemaps).</para>
        /// <para>When false, the URLs aren’t saved in the configuration.</para>
        /// <para>When unspecified, the URLs are added to the extraUrls list of your configuration, but only if they haven’t been indexed during the
        /// last reindex, and they aren’t already present in startUrls or sitemaps.</para>
        /// </summary>
        [JsonProperty("save", NullValueHandling = NullValueHandling.Ignore)]
        public bool Save
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CrawlUrlsBody"/> class.
        /// </summary>
        /// <param name="urls">The URLs to crawl.</param>
        public CrawlUrlsBody(IEnumerable<string> urls)
        {
            Urls = urls;
        }
    }
}
