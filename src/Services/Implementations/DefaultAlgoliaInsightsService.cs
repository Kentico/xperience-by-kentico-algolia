using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Insights;
using Algolia.Search.Models.Search;

using CMS.ContactManagement;
using CMS.Core;
using CMS.Helpers;

using Kentico.Xperience.Algolia.Models;

using Microsoft.Extensions.Options;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaInsightsService"/> which logs
    /// Algolia Insights events using the <see cref="ContactInfo.ContactGUID"/>
    /// as the user's identifier.
    /// </summary>
    internal class DefaultAlgoliaInsightsService : IAlgoliaInsightsService
    {
        private readonly AlgoliaOptions algoliaOptions;
        private readonly IInsightsClient insightsClient;
        private readonly IEventLogService eventLogService;
        private readonly Regex queryParameterRegex = new ("^[a-fA-F0-9]{32}$");
        

        private string ContactGUID
        {
            get
            {
                var currentContact = ContactManagementContext.CurrentContact;
                if (currentContact == null)
                {
                    return string.Empty;
                }

                return currentContact.ContactGUID.ToString();
            }
        }


        private string ObjectId
        {
            get
            {
                return QueryHelper.GetString(algoliaOptions.ObjectIdParameterName, String.Empty);
            }
        }


        private string QueryId
        {
            get
            {
                var value = QueryHelper.GetString(algoliaOptions.QueryIdParameterName, String.Empty);
                if (queryParameterRegex.IsMatch(value))
                {
                    return value;
                }

                return String.Empty;
            }
        }


        private uint Position
        {
            get
            {
                return (uint)QueryHelper.GetInteger(algoliaOptions.PositionParameterName, 0);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaInsightsService"/> class.
        /// </summary>
        public DefaultAlgoliaInsightsService(IOptions<AlgoliaOptions> algoliaOptions,
            IInsightsClient insightsClient,
            IEventLogService eventLogService)
        {
            this.algoliaOptions = algoliaOptions.Value;
            this.insightsClient = insightsClient;
            this.eventLogService = eventLogService;
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogSearchResultClicked(string eventName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(ObjectId) || String.IsNullOrEmpty(QueryId) || String.IsNullOrEmpty(indexName) || String.IsNullOrEmpty(eventName) || Position <= 0)
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ClickedObjectIDsAfterSearchAsync(eventName, indexName, new string[] { ObjectId }, new uint[] { Position }, QueryId, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogSearchResultClicked), ex);
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogSearchResultConversion(string conversionName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(ObjectId) || String.IsNullOrEmpty(QueryId) || String.IsNullOrEmpty(indexName) || String.IsNullOrEmpty(conversionName))
            {
                return new InsightsResponse() {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ConvertedObjectIDsAfterSearchAsync(conversionName, indexName, new string[] { ObjectId }, QueryId, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogSearchResultConversion), ex);
                return new InsightsResponse() {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogPageViewed(int documentId, string eventName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(indexName) || String.IsNullOrEmpty(eventName) || documentId <= 0)
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ViewedObjectIDsAsync(eventName, indexName, new string[] { documentId.ToString() }, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogPageViewed), ex);
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogPageConversion(int documentId, string conversionName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(indexName) || String.IsNullOrEmpty(conversionName) || documentId <= 0)
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ConvertedObjectIDsAsync(conversionName, indexName, new string[] { documentId.ToString() }, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogPageConversion), ex);
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogFacetsViewed(IEnumerable<AlgoliaFacetedAttribute> facets, string eventName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || facets == null)
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            var viewedFacets = new List<string>();
            foreach(var facetedAttribute in facets)
            {
                viewedFacets.AddRange(facetedAttribute.Facets.Select(facet => $"{facet.Attribute}:{facet.Value}"));
            }

            if (viewedFacets.Count > 0)
            {
                try
                {
                    return await insightsClient.User(ContactGUID).ViewedFiltersAsync(eventName, indexName, viewedFacets, ct: cancellationToken);
                }
                catch (Exception ex)
                {
                    eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogFacetsViewed), ex);
                    return new InsightsResponse()
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Message = ex.Message
                    };
                }
            }

            return new InsightsResponse()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Message = "No facets were provided."
            };
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogFacetClicked(string facet, string eventName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(facet) || String.IsNullOrEmpty(eventName) || String.IsNullOrEmpty(indexName))
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ClickedFiltersAsync(eventName, indexName, new string[] { facet }, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogFacetClicked), ex);
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public async Task<InsightsResponse> LogFacetConverted(string facet, string conversionName, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(ContactGUID) || String.IsNullOrEmpty(facet) || String.IsNullOrEmpty(conversionName) || String.IsNullOrEmpty(indexName))
            {
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "One or more parameters are invalid."
                };
            }

            try
            {
                return await insightsClient.User(ContactGUID).ConvertedFiltersAsync(conversionName, indexName, new string[] { facet }, ct: cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaInsightsService), nameof(LogFacetConverted), ex);
                return new InsightsResponse()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <inheritdoc />
        public void SetInsightsUrls<TModel>(SearchResponse<TModel> searchResponse) where TModel : AlgoliaSearchModel
        {
            for (var i = 0; i < searchResponse.Hits.Count; i++)
            {
                var position = i + 1 + (searchResponse.HitsPerPage * searchResponse.Page);
                searchResponse.Hits[i].Url = GetInsightsUrl(searchResponse.Hits[i], position, searchResponse.QueryID);
            }
        }


        /// <summary>
        /// Gets the Algolia hit's absolute URL with the appropriate query string parameters
        /// populated to log search result click events.
        /// </summary>
        /// <typeparam name="TModel">The type of the Algolia search model.</typeparam>
        /// <param name="hit">The Algolia hit to retrieve the URL for.</param>
        /// <param name="position">The position the <paramref name="hit"/> appeared in the
        /// search results.</param>
        /// <param name="queryId">The unique identifier of the Algolia query.</param>
        private string GetInsightsUrl<TModel>(TModel hit, int position, string queryId) where TModel : AlgoliaSearchModel
        {
            var url = hit.Url;
            url = URLHelper.AddQueryParameter(url, algoliaOptions.ObjectIdParameterName, hit.ObjectID);
            url = URLHelper.AddQueryParameter(url, algoliaOptions.PositionParameterName, position.ToString());
            url = URLHelper.AddQueryParameter(url, algoliaOptions.QueryIdParameterName, queryId);

            return url;
        }
    }
}
