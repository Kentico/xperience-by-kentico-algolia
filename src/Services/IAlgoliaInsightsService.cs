using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Models.Insights;
using Algolia.Search.Models.Search;

using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Contains methods for logging Algolia Insights events.
    /// </summary>
    /// <remarks>See <see href="https://www.algolia.com/doc/guides/getting-analytics/search-analytics/advanced-analytics/"/>.</remarks>
    public interface IAlgoliaInsightsService
    {
        /// <summary>
        /// Logs a search result click event. Required query parameters must be present in the
        /// request, or no event is logged.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogSearchResultClicked(string eventName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs a search result click conversion. Required query parameters must be present in the
        /// request, or no event is logged.
        /// </summary>
        /// <param name="conversionName">The name of the converstion.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogSearchResultConversion(string conversionName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs a conversion that didn't occur after an Algolia search.
        /// </summary>
        /// <param name="documentId">The <see cref="TreeNode.DocumentID"/> page that the conversion
        /// occurred on.</param>
        /// <param name="conversionName">The name of the conversion.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogPageConversion(int documentId, string conversionName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs an event when a visitor views a page contained within the Algolia index, but not after
        /// a search.
        /// </summary>
        /// <param name="documentId">>The <see cref="TreeNode.DocumentID"/> page that the conversion
        /// occurred on.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogPageViewed(int documentId, string eventName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs an event when a visitor views search facets but didn't click on them.
        /// </summary>
        /// <param name="facets">The facets that were displayed to the visitor.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogFacetsViewed(IEnumerable<AlgoliaFacetedAttribute> facets, string eventName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs an event when a visitor clicks a facet.
        /// </summary>
        /// <param name="facet">The facet name and value, e.g. "CoffeeIsDecaf:true."</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogFacetClicked(string facet, string eventName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Logs a conversion when a visitor clicks a facet.
        /// </summary>
        /// <param name="facet">The facet name and value, e.g. "CoffeeIsDecaf:true."</param>
        /// <param name="conversionName">The name of the conversion.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A response object containing the status code and message from the server.</returns>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<InsightsResponse> LogFacetConverted(string facet, string conversionName, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Updates the <see cref="AlgoliaSearchModel.Url"/> property of all search results
        /// with the query parameters needed to track search result click and conversion events.
        /// </summary>
        /// <typeparam name="TModel">The type of the Algolia search model.</typeparam>
        /// <param name="searchResponse">The full response of an Algolia search.</param>
        void SetInsightsUrls<TModel>(SearchResponse<TModel> searchResponse) where TModel : AlgoliaSearchModel;
    }
}
