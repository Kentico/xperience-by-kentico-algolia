using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Newtonsoft.Json;

namespace Kentico.Xperience.Algolia.Tests
{
    /// <summary>
    /// Custom <see cref="HttpMessageHandler"/> which can be used when initializing an <see cref="HttpClient"/>
    /// to fake responses.
    /// </summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        /// A fake response for the <see cref="DefaultAlgoliaClient.GetCrawler"/> request.
        /// </summary>
        public static AlgoliaCrawler TestCrawlerResponse
        {
            get
            {
                return new AlgoliaCrawler
                {
                    Name = "TEST",
                    Config = new AlgoliaCrawlerConfig
                    {
                        IndexPrefix = "PREFIX_"
                    }
                };
            }
        }


        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(MockSend(request, cancellationToken));
        }


        /// <inheritdoc/>
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return MockSend(request, cancellationToken);
        }


        /// <summary>
        /// Called in place of actual HTTP requests. Allows for faking responses from Algolia's REST API.
        /// </summary>
        /// <param name="request">The request that would be sent to Algolia.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>A faked response, generally an empty 200 response.</returns>
        public virtual HttpResponseMessage MockSend(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var getCrawlerUrl = String.Format(DefaultAlgoliaClient.BASE_URL + DefaultAlgoliaClient.PATH_GET_CRAWLER, TestSearchModels.CRAWLER_ID);
            if (request.RequestUri.AbsoluteUri.Equals(getCrawlerUrl, StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(TestCrawlerResponse))
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
