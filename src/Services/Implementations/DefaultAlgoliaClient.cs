using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Models;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaClient"/>.
    /// </summary>
    internal class DefaultAlgoliaClient : IAlgoliaClient
    {
        private readonly AlgoliaOptions algoliaOptions;
        private readonly HttpClient httpClient;
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly ICacheAccessor cacheAccessor;
        private readonly IEventLogService eventLogService;
        private readonly IPageRetriever pageRetriever;
        private readonly IProgressiveCache progressiveCache;
        private readonly ISearchClient searchClient;
        private const string CACHEKEY_CRAWLER = "Algolia|Crawler|{0}";


        internal const string BASE_URL = "https://crawler.algolia.com/api/1/";
        internal const string PATH_CRAWL_URLS = "crawlers/{0}/urls/crawl";
        internal const string PATH_GET_CRAWLER = "crawlers/{0}?withConfig=true";
        internal const string CACHEKEY_STATISTICS = "Algolia|ListIndices";


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(HttpClient httpClient,
            IAlgoliaIndexService algoliaIndexService,
            ICacheAccessor cacheAccessor,
            IEventLogService eventLogService,
            IPageRetriever pageRetriever,
            IProgressiveCache progressiveCache,
            ISearchClient searchClient,
            IOptions<AlgoliaOptions> options)
        {
            algoliaOptions = options.Value;
            this.httpClient = httpClient;
            this.algoliaIndexService = algoliaIndexService;
            this.cacheAccessor = cacheAccessor;
            this.eventLogService = eventLogService;
            this.pageRetriever = pageRetriever;
            this.progressiveCache = progressiveCache;
            this.searchClient = searchClient;

            // Initialize HttpClient used for crawler requests if a crawler is registered
            if (IndexStore.Instance.GetAllCrawlers().Any())
            {
                httpClient.BaseAddress = new Uri(BASE_URL);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {GetBasicAuthentication()}");
            }
        }


        /// <inheritdoc/>
        public Task<int> CrawlUrls(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(crawlerId))
            {
                throw new ArgumentNullException(nameof(crawlerId));
            }

            if (urls == null || !urls.Any())
            {
                throw new InvalidOperationException("No URLs were provided.");
            }

            return CrawlUrlsInternal(crawlerId, urls, cancellationToken);
        }


        /// <inheritdoc/>
        public Task<int> DeleteUrls(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(crawlerId))
            {
                throw new ArgumentNullException(nameof(crawlerId));
            }

            if (urls == null || !urls.Any())
            {
                throw new InvalidOperationException("No URLs were provided.");
            }

            return DeleteUrlsInternal(crawlerId, urls, cancellationToken);
        }


        /// <inheritdoc/>
        public Task<AlgoliaCrawler> GetCrawler(string crawlerId, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(crawlerId))
            {
                throw new ArgumentNullException(nameof(crawlerId));
            }

            return GetCrawlerInternal(crawlerId, cancellationToken);
        }


        /// <inheritdoc />
        public Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (objectIds == null || !objectIds.Any())
            {
                return Task.FromResult(0);
            }

            return DeleteRecordsInternal(objectIds, indexName, cancellationToken);
        }


        /// <inheritdoc/>
        public async Task<ICollection<IndicesResponse>> GetStatistics(CancellationToken cancellationToken)
        {
            return await progressiveCache.LoadAsync(async (cs, ct) => {
                var response = await searchClient.ListIndicesAsync(ct: ct).ConfigureAwait(false);
                return response.Items;
            }, new CacheSettings(20, CACHEKEY_STATISTICS), cancellationToken).ConfigureAwait(false);
        }


        /// <inheritdoc />
        public Task Rebuild(string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"The index '{indexName}' is not registered.");
            }

            return RebuildInternal(algoliaIndex, cancellationToken);
        }


        /// <inheritdoc />
        public Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (dataObjects == null || !dataObjects.Any())
            {
                return Task.FromResult(0);
            }

            return UpsertRecordsInternal(dataObjects, indexName, cancellationToken);
        }


        private async Task<int> CrawlUrlsInternal(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            var path = String.Format(PATH_CRAWL_URLS, crawlerId);
            var body = new CrawlUrlsBody(urls);
            var data = new StringContent(JsonConvert.SerializeObject(body), null, "application/json");
            var response = await SendRequest(path, HttpMethod.Post, cancellationToken, data);
            if (response == null)
            {
                return 0;
            }

            return urls.Count();
        }


        private async Task<int> DeleteUrlsInternal(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken)
        {
            var crawlerDetail = await GetCrawler(crawlerId, cancellationToken);
            if (crawlerDetail == null)
            {
                return 0;
            }

            var deletedCount = 0;
            var searchIndex = algoliaIndexService.InitializeCrawler(crawlerDetail);
            var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(urls, ct: cancellationToken);
            foreach (var response in batchIndexingResponse.Responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }


        private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken)
        {
            var deletedCount = 0;
            var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
            var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(objectIds, ct: cancellationToken).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }


        private string GetBasicAuthentication()
        {
            if (String.IsNullOrEmpty(algoliaOptions.CrawlerUserId) || String.IsNullOrEmpty(algoliaOptions.CrawlerApiKey))
            {
                throw new InvalidOperationException("The Algolia crawler configuration is invalid.");
            }

            var bytes = Encoding.UTF8.GetBytes($"{algoliaOptions.CrawlerUserId}:{algoliaOptions.CrawlerApiKey}");
            return Convert.ToBase64String(bytes);
        }


        private async Task<AlgoliaCrawler> GetCrawlerInternal(string crawlerId, CancellationToken cancellationToken)
        {
            return await progressiveCache.LoadAsync(async (cs, ct) => {
                var path = String.Format(PATH_GET_CRAWLER, crawlerId);
                var response = await SendRequest(path, HttpMethod.Get, ct);
                if (response == null)
                {
                    cs.Cached = false;
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(ct);

                return JsonConvert.DeserializeObject<AlgoliaCrawler>(content);
            }, new CacheSettings(20, String.Format(CACHEKEY_CRAWLER, crawlerId)), cancellationToken).ConfigureAwait(false);
        }


        private async Task RebuildInternal(AlgoliaIndex algoliaIndex, CancellationToken cancellationToken)
        {
            // Clear statistics cache so listing displays updated data after rebuild
            cacheAccessor.Remove(CACHEKEY_STATISTICS);
            
            var indexedNodes = new List<TreeNode>();
            foreach (var includedPathAttribute in algoliaIndex.IncludedPaths)
            {
                var nodes = await pageRetriever.RetrieveMultipleAsync(q =>
                {
                    if (includedPathAttribute.ContentTypes.Length > 0)
                    {
                        q.Types(includedPathAttribute.ContentTypes);
                    }

                    q.Path(includedPathAttribute.AliasPath)
                        .PublishedVersion()
                        .WithCoupledColumns();
                }, cancellationToken: cancellationToken);
                    
                indexedNodes.AddRange(nodes);
            }

            var searchIndex = await algoliaIndexService.InitializeIndex(algoliaIndex.IndexName, cancellationToken);
            await searchIndex.ClearObjectsAsync(ct: cancellationToken);

            indexedNodes.ForEach(node => AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(new AlgoliaQueueItem(node, AlgoliaTaskType.CREATE, algoliaIndex.IndexName)));
        }


        private async Task<HttpResponseMessage> SendRequest(string path, HttpMethod method, CancellationToken cancellationToken, HttpContent data = null)
        {
            if (method == HttpMethod.Post && data == null)
            {
                eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(SendRequest), "Data must be provided for the POST method.");
                return null;
            }

            HttpResponseMessage response = null;
            try
            {
                if (method.Equals(HttpMethod.Get))
                {
                    response = await httpClient.GetAsync(path, cancellationToken);
                }
                else if (method.Equals(HttpMethod.Post))
                {
                    // Algolia throws 415 if charset is specified
                    data.Headers.ContentType.CharSet = String.Empty;
                    response = await httpClient.PostAsync(path, data, cancellationToken);
                }
                else
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(SendRequest), $"Unsupported HTTP method {nameof(method)}");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(SendRequest),
                        $"Request for {path} returned {response.StatusCode}: {content}");

                    return null;
                }

                return response;
            }
            catch (Exception ex)
            {
                eventLogService.LogException(nameof(DefaultAlgoliaClient), nameof(SendRequest), ex);
                return null;
            }
        }


        private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken)
        {
            var upsertedCount = 0;
            var searchIndex = await algoliaIndexService.InitializeIndex(indexName, cancellationToken);
            var batchIndexingResponse = await searchIndex.PartialUpdateObjectsAsync(dataObjects, createIfNotExists: true, ct: cancellationToken).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                upsertedCount += response.ObjectIDs.Count();
            }

            return upsertedCount;
        }
    }
}
