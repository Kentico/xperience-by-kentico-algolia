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
    public class MockHttpMessageHandler : HttpMessageHandler
    {
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


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(MockSend(request, cancellationToken));
        }


        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return MockSend(request, cancellationToken);
        }


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
