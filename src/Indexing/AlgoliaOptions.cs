﻿using Kentico.Xperience.Algolia.Search;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// Algolia integration options.
    /// </summary>
    public sealed class AlgoliaOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SECTION_NAME = "xperience.algolia";


        /// <summary>
        /// Algolia application ID.
        /// </summary>
        public string ApplicationId
        {
            get;
            set;
        } = "NO_APP";


        /// <summary>
        /// Algolia API key.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        } = "NO_KEY";


        /// <summary>
        /// Public API key used for searching only.
        /// </summary>
        public string SearchKey
        {
            get;
            set;
        }


        /// <summary>
        /// The Algolia crawler API key.
        /// </summary>
        public string CrawlerApiKey
        {
            get;
            set;
        }


        /// <summary>
        /// The Algolia crawler user ID.
        /// </summary>
        public string CrawlerUserId
        {
            get;
            set;
        }


        /// <summary>
        /// The interval at which <see cref="AlgoliaCrawlerQueueWorker"/> runs, in milliseconds.
        /// </summary>
        public int CrawlerInterval
        {
            get;
            set;
        }


        /// <summary>
        /// The query string parameter name which stores a search result's <see cref="AlgoliaSearchModel.ObjectID"/>.
        /// Used by the <see cref="IAlgoliaInsightsService.SetInsightsUrls"/> to set search results URLs,
        /// and is logged by <see cref="IAlgoliaInsightsService.LogSearchResultClicked"/>
        /// and <see cref="IAlgoliaInsightsService.LogSearchResultConversion"/>.
        /// </summary>
        public string ObjectIdParameterName
        {
            get;
            set;
        } = "object";


        /// <summary>
        /// The query string parameter name which stores an Algolia search query ID.
        /// Used by the <see cref="IAlgoliaInsightsService.SetInsightsUrls"/> to set search results URLs,
        /// and is logged by <see cref="IAlgoliaInsightsService.LogSearchResultClicked"/>
        /// and <see cref="IAlgoliaInsightsService.LogSearchResultConversion"/>.
        /// </summary>
        public string QueryIdParameterName
        {
            get;
            set;
        } = "query";


        /// <summary>
        /// The query string parameter name which stores a search result's position in the response.
        /// Used by the <see cref="IAlgoliaInsightsService.SetInsightsUrls"/> to set search results URLs,
        /// and is logged by <see cref="IAlgoliaInsightsService.LogSearchResultClicked"/>
        /// and <see cref="IAlgoliaInsightsService.LogSearchResultConversion"/>.
        /// </summary>
        public string PositionParameterName
        {
            get;
            set;
        } = "pos";
    }
}