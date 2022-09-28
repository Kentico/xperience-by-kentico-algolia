using System.Collections.Generic;

using Newtonsoft.Json;

namespace Kentico.Xperience.Algolia.Models
{
    internal class CrawlUrlsBody
    {
        [JsonProperty("urls")]
        public IEnumerable<string> Urls
        {
            get;
        }


        /// <summary>
        /// When true, the given URLs are added to the extraUrls list of your configuration (unless already present in startUrls or sitemaps).
        /// When false, the URLs aren’t saved in the configuration.
        /// When unspecified, the URLs are added to the extraUrls list of your configuration, but only if they haven’t been indexed during the last reindex, and they aren’t already present in startUrls or sitemaps.
        /// </summary>
        [JsonProperty("save", NullValueHandling = NullValueHandling.Ignore)]
        public bool Save
        {
            get;
            set;
        }


        public CrawlUrlsBody(IEnumerable<string> urls)
        {
            Urls = urls;
        }
    }
}
