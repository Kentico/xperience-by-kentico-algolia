using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaClientTests
    {
        private static IOptions<AlgoliaOptions> GetMockAlgoliaOptions()
        {
            var mockOptions = Substitute.For<IOptions<AlgoliaOptions>>();
            mockOptions.Value.Returns(new AlgoliaOptions
            {
                CrawlerUserId = "CRAWLER_USER",
                CrawlerApiKey = "CRAWLER_KEY"
            });

            return mockOptions;
        }


        private static ISearchIndex GetMockSearchIndex()
        {
            var mockSearchIndex = Substitute.For<ISearchIndex>();
            mockSearchIndex.DeleteObjectsAsync(Arg.Any<IEnumerable<string>>(), null, Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(args => Task.FromResult(new BatchIndexingResponse
                {
                    Responses = new List<BatchResponse>
                    {
                        new BatchResponse
                        {
                            ObjectIDs = args.Arg<IEnumerable<string>>()
                        }
                    }
                }
            ));
            mockSearchIndex.PartialUpdateObjectsAsync(Arg.Any<IEnumerable<JObject>>(), null, Arg.Any<CancellationToken>(), Arg.Any<bool>())
                .ReturnsForAnyArgs(args => Task.FromResult(new BatchIndexingResponse
                {
                    Responses = new List<BatchResponse>
                    {
                        new BatchResponse
                        {
                            ObjectIDs = new string[args.Arg<IEnumerable<JObject>>().Count()]
                        }
                    }
                }
            ));

            return mockSearchIndex;
        }


        [TestFixture]
        internal class CrawlUrlsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly MockHttpMessageHandler mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();


            [SetUp]
            public void CrawlUrlsTestsSetUp()
            {
                var httpClient = new HttpClient(mockHttpMessageHandler)
                {
                    BaseAddress = new Uri(DefaultAlgoliaClient.BASE_URL)
                };

                algoliaClient = new DefaultAlgoliaClient(httpClient,
                    Substitute.For<IAlgoliaIndexService>(),
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task CrawlUrls_HttpClientReceivesParameters()
            {
                var crawledUrls = new string[] { "https://test" };
                var expectedContent = JsonConvert.SerializeObject(new CrawlUrlsBody(crawledUrls));
                var expectedUrl = String.Format(DefaultAlgoliaClient.BASE_URL + DefaultAlgoliaClient.PATH_CRAWL_URLS, CRAWLER_ID);

                await algoliaClient.CrawlUrls(CRAWLER_ID, crawledUrls, CancellationToken.None);

                mockHttpMessageHandler.Received(1).MockSend(Arg.Is<HttpRequestMessage>(arg =>
                    arg.Method == HttpMethod.Post &&
                    arg.RequestUri.AbsoluteUri.Equals(expectedUrl, StringComparison.OrdinalIgnoreCase) &&
                    arg.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult().Equals(expectedContent, StringComparison.OrdinalIgnoreCase)
                ), Arg.Any<CancellationToken>());
            }
        }


        [TestFixture]
        internal class DeleteRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();
            private readonly IAlgoliaIndexService mockIndexService = Substitute.For<IAlgoliaIndexService>();
            private readonly IAlgoliaObjectGenerator algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                Substitute.For<IEventLogService>(),
                Substitute.For<IMediaFileInfoProvider>(),
                Substitute.For<IMediaFileUrlRetriever>());


            [SetUp]
            public void DeleteRecordsTestsSetUp()
            {
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);

                algoliaClient = new DefaultAlgoliaClient(Substitute.For<HttpClient>(),
                    mockIndexService,
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task DeleteRecords_ValidIndex_ReturnsProcessedCount()
            {
                var cancellationToken = new CancellationToken();
                var enQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var objectIdEn = algoliaObjectGenerator.GetTreeNodeData(enQueueItem).Value<string>("objectID");
                var czQueueItem = new AlgoliaQueueItem( FakeNodes.ArticleCz, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var objectIdCz = algoliaObjectGenerator.GetTreeNodeData(czQueueItem).Value<string>("objectID");
                var objectIds = new string[] { objectIdEn, objectIdCz };
                var numProcessed = await algoliaClient.DeleteRecords(objectIds, nameof(ArticleEnSearchModel), cancellationToken);

                Assert.That(numProcessed, Is.EqualTo(2));
                await mockIndexService.Received(1).InitializeIndex(nameof(ArticleEnSearchModel), cancellationToken);
                await mockSearchIndex.Received(1).DeleteObjectsAsync(Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(objectIds)), null, cancellationToken);
            }
        }


        [TestFixture]
        internal class DeleteUrlsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();
            private readonly IProgressiveCache mockProgressiveCache = Substitute.For<IProgressiveCache>();
            private readonly IAlgoliaIndexService mockIndexService = Substitute.For<IAlgoliaIndexService>();
            private readonly MockHttpMessageHandler mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();


            [SetUp]
            public void DeleteUrlsTestsSetUp()
            {
                mockIndexService.InitializeCrawler(Arg.Any<AlgoliaCrawler>()).ReturnsForAnyArgs(mockSearchIndex);
                mockProgressiveCache.LoadAsync(Arg.Any<Func<CacheSettings, CancellationToken, Task<AlgoliaCrawler>>>(), Arg.Any<CacheSettings>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async args =>
                {
                    // Execute the passed function
                    var func = args.ArgAt<Func<CacheSettings, CancellationToken, Task<AlgoliaCrawler>>>(0);
                    return await func(args.ArgAt<CacheSettings>(1), args.ArgAt<CancellationToken>(2));
                });
                var httpClient = new HttpClient(mockHttpMessageHandler)
                {
                    BaseAddress = new Uri(DefaultAlgoliaClient.BASE_URL)
                };

                algoliaClient = new DefaultAlgoliaClient(httpClient,
                    mockIndexService,
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    mockProgressiveCache,
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task DeleteUrls_ReturnsProcessedCount()
            {
                var cancellationToken = new CancellationToken();
                var deletedUrls = new string[] { "https://test" };
                var numProcessed = await algoliaClient.DeleteUrls(CRAWLER_ID, deletedUrls, cancellationToken);

                Assert.That(numProcessed, Is.EqualTo(1));
                await mockSearchIndex.Received(1).DeleteObjectsAsync(Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(deletedUrls)), null, cancellationToken);
                mockIndexService.Received(1).InitializeCrawler(Arg.Is<AlgoliaCrawler>(arg =>
                    arg.Name.Equals(MockHttpMessageHandler.TestCrawlerResponse.Name, StringComparison.OrdinalIgnoreCase) &&
                    arg.Config.IndexPrefix.Equals(MockHttpMessageHandler.TestCrawlerResponse.Config.IndexPrefix, StringComparison.OrdinalIgnoreCase)
                ));
            }
        }


        [TestFixture]
        internal class GetCrawlerTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly IProgressiveCache mockProgressiveCache = Substitute.For<IProgressiveCache>();
            private readonly MockHttpMessageHandler mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();


            [SetUp]
            public void GetCrawlerTestsSetUp()
            {
                mockProgressiveCache.LoadAsync(Arg.Any<Func<CacheSettings, CancellationToken, Task<AlgoliaCrawler>>>(), Arg.Any<CacheSettings>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async args =>
                {
                    // Execute the passed function
                    var func = args.ArgAt<Func<CacheSettings, CancellationToken, Task <AlgoliaCrawler>>>(0);
                    return await func(args.ArgAt<CacheSettings>(1), args.ArgAt<CancellationToken>(2));
                });
                var httpClient = new HttpClient(mockHttpMessageHandler)
                {
                    BaseAddress = new Uri(DefaultAlgoliaClient.BASE_URL)
                };

                algoliaClient = new DefaultAlgoliaClient(httpClient,
                    Substitute.For<IAlgoliaIndexService>(),
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    mockProgressiveCache,
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task GetCrawler_CallsMethods()
            {
                var cancellationToken = new CancellationToken();
                var expectedUrl = String.Format(DefaultAlgoliaClient.BASE_URL + DefaultAlgoliaClient.PATH_GET_CRAWLER, CRAWLER_ID);
                var crawler = await algoliaClient.GetCrawler(CRAWLER_ID, cancellationToken);

                Assert.That(crawler, Is.Not.Null);
                await mockProgressiveCache.Received(1).LoadAsync(Arg.Any<Func<CacheSettings, CancellationToken, Task<AlgoliaCrawler>>>(), Arg.Any<CacheSettings>(), cancellationToken);
                mockHttpMessageHandler.Received(1).MockSend(Arg.Is<HttpRequestMessage>(arg =>
                    arg.Method == HttpMethod.Get &&
                    arg.RequestUri.AbsoluteUri.Equals(expectedUrl, StringComparison.OrdinalIgnoreCase)
                ), Arg.Any<CancellationToken>());
            }
        }


        [TestFixture]
        internal class GetStatisticsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly IProgressiveCache mockProgressiveCache = Substitute.For<IProgressiveCache>();
            private readonly ISearchClient mockSearchClient = Substitute.For<ISearchClient>();
            

            [SetUp]
            public void GetStatisticsTestsSetUp()
            {
                mockSearchClient.ListIndicesAsync(null, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(args => Task.FromResult(new ListIndicesResponse()));
                mockProgressiveCache.LoadAsync(Arg.Any<Func<CacheSettings, CancellationToken, Task<List<IndicesResponse>>>>(), Arg.Any<CacheSettings>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(async args =>
                {
                    // Execute the passed function
                    var func = args.ArgAt<Func<CacheSettings, CancellationToken, Task<List<IndicesResponse>>>>(0);
                    return await func(args.ArgAt<CacheSettings>(1), args.ArgAt<CancellationToken>(2));
                });

                algoliaClient = new DefaultAlgoliaClient(Substitute.For<HttpClient>(),
                    Substitute.For<IAlgoliaIndexService>(),
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    mockProgressiveCache,
                    mockSearchClient,
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task GetStatistics_CallsMethods()
            {
                var cancellationToken = new CancellationToken();

                await algoliaClient.GetStatistics(cancellationToken);
                await mockSearchClient.Received(1).ListIndicesAsync(null, cancellationToken);
                await mockProgressiveCache.Received(1).LoadAsync(Arg.Any<Func<CacheSettings, CancellationToken, Task<List<IndicesResponse>>>>(), Arg.Any<CacheSettings>(), cancellationToken);
            }
        }


        [TestFixture]
        internal class RebuildTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();
            private readonly ICacheAccessor mockCacheAccessor = Substitute.For<ICacheAccessor>();
            private readonly IAlgoliaIndexService mockIndexService = Substitute.For<IAlgoliaIndexService>();
            private readonly IPageRetriever mockPageRetriever = Substitute.For<IPageRetriever>();
            private readonly IAlgoliaTaskProcessor mockTaskProcessor = Substitute.For<IAlgoliaTaskProcessor>();


            protected override void RegisterTestServices()
            {
                base.RegisterTestServices();

                Service.Use<IAlgoliaTaskProcessor>(mockTaskProcessor);
            }


            [SetUp]
            public void RebuildTestsSetUp()
            {
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);
                mockPageRetriever.RetrieveMultipleAsync(Arg.Any<Action<MultiDocumentQuery>>(), null, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(
                    new TreeNode[] { FakeNodes.ArticleEn });

                algoliaClient = new DefaultAlgoliaClient(Substitute.For<HttpClient>(),
                    mockIndexService,
                    mockCacheAccessor,
                    Substitute.For<IEventLogService>(),
                    mockPageRetriever,
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public void Rebuild_InvalidParameters_Throws()
            {
                Assert.Multiple(() => {
                    Assert.ThrowsAsync<ArgumentNullException>(async () => await algoliaClient.Rebuild(null, CancellationToken.None));
                    Assert.ThrowsAsync<ArgumentNullException>(async () => await algoliaClient.Rebuild(String.Empty, CancellationToken.None));
                    Assert.ThrowsAsync<InvalidOperationException>(async () => await algoliaClient.Rebuild("NO_INDEX", CancellationToken.None));
                });
            }


            [Test]
            public async Task Rebuild_ValidIndex_CallsMethods()
            {
                var cancellationToken = new CancellationToken();
                await algoliaClient.Rebuild(nameof(ArticleEnSearchModel), cancellationToken);

                mockCacheAccessor.Received(1).Remove(DefaultAlgoliaClient.CACHEKEY_STATISTICS);
                await mockPageRetriever.Received(1).RetrieveMultipleAsync(Arg.Any<Action<MultiDocumentQuery>>(), null, cancellationToken);
                await mockIndexService.Received(1).InitializeIndex(nameof(ArticleEnSearchModel), cancellationToken);
                await mockSearchIndex.Received(1).ClearObjectsAsync(ct: cancellationToken);
                await mockTaskProcessor.Received(1).ProcessAlgoliaTasks(Arg.Is<IEnumerable<AlgoliaQueueItem>>(arg =>
                    arg.Count() == 1 &&
                    arg.FirstOrDefault().ChangedColumns == null &&
                    arg.FirstOrDefault().Node == FakeNodes.ArticleEn &&
                    arg.FirstOrDefault().TaskType == AlgoliaTaskType.CREATE &&
                    arg.FirstOrDefault().IndexName.Equals(nameof(ArticleEnSearchModel), StringComparison.OrdinalIgnoreCase)),
                cancellationToken);
            }
        }


        [TestFixture]
        internal class UpsertRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();
            private readonly IAlgoliaIndexService mockIndexService = Substitute.For<IAlgoliaIndexService>();
            private readonly IAlgoliaObjectGenerator algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                Substitute.For<IEventLogService>(),
                Substitute.For<IMediaFileInfoProvider>(),
                Substitute.For<IMediaFileUrlRetriever>());


            [SetUp]
            public void UpsertRecordsTestsSetUp()
            {
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);

                algoliaClient = new DefaultAlgoliaClient(Substitute.For<HttpClient>(),
                    mockIndexService,
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>(),
                    GetMockAlgoliaOptions());
            }


            [Test]
            public async Task UpsertRecords_ValidIndex_ReturnsProcessedCount()
            {
                var cancellationToken = new CancellationToken();
                var enQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var czQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var dataToUpsert = new JObject[] {
                    algoliaObjectGenerator.GetTreeNodeData(enQueueItem),
                    algoliaObjectGenerator.GetTreeNodeData(czQueueItem)
                };
                var numProcessed = await algoliaClient.UpsertRecords(dataToUpsert, nameof(ArticleEnSearchModel), cancellationToken);

                Assert.That(numProcessed, Is.EqualTo(2));
                await mockIndexService.Received(1).InitializeIndex(nameof(ArticleEnSearchModel), cancellationToken);
                await mockSearchIndex.Received(1).PartialUpdateObjectsAsync(
                    Arg.Is<IEnumerable<JObject>>(arg => arg.SequenceEqual(dataToUpsert, new JObjectEqualityComparer())), createIfNotExists: true, ct: cancellationToken);
            }
        }
    }
}
