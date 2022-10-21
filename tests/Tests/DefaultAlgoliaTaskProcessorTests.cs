using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers.Caching.Abstractions;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.WorkflowEngine;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaTaskProcessorTests
    {
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
        internal class ProcessAlgoliaTasksTests : AlgoliaTests
        {
            private IAlgoliaTaskProcessor algoliaTaskProcessor;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();
            private readonly IAlgoliaObjectGenerator algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                Substitute.For<IEventLogService>(),
                Substitute.For<IMediaFileInfoProvider>(),
                Substitute.For<IMediaFileUrlRetriever>());


            [SetUp]
            public void ProcessAlgoliaTasksTestsSetUp()
            {
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);

                var mockOptions = Substitute.For<IOptions<AlgoliaOptions>>();
                mockOptions.Value.Returns(new AlgoliaOptions
                {
                    CrawlerUserId = "CRAWLER_USER",
                    CrawlerApiKey = "CRAWLER_KEY"
                });

                var mockAlgoliaClient = new DefaultAlgoliaClient(Substitute.For<HttpClient>(),
                    mockIndexService,
                    Substitute.For<IAlgoliaObjectGenerator>(),
                    Substitute.For<ICacheAccessor>(),
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IPageRetriever>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>(),
                    mockOptions);

                algoliaTaskProcessor = new DefaultAlgoliaTaskProcessor(mockAlgoliaClient,
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    algoliaObjectGenerator);
            }


            [Test]
            public async Task ProcessAlgoliaTasks_ValidTasks_ProcessesTasks()
            {
                var cancellationToken = new CancellationToken();
                var createQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var deleteQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var updateQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.UPDATE, nameof(ArticleEnSearchModel), new string[] { "DocumentName" });
                var IdToDelete = algoliaObjectGenerator.GetTreeNodeData(deleteQueueItem).Value<string>("objectID");
                var dataToUpsert = new JObject[] {
                    algoliaObjectGenerator.GetTreeNodeData(createQueueItem),
                    algoliaObjectGenerator.GetTreeNodeData(updateQueueItem)
                };
                var numProcessed = await algoliaTaskProcessor.ProcessAlgoliaTasks(new AlgoliaQueueItem[] { createQueueItem, updateQueueItem, deleteQueueItem }, cancellationToken);

                Assert.That(numProcessed, Is.EqualTo(3));
                await mockSearchIndex.Received(1).DeleteObjectsAsync(Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(new string[] { IdToDelete })), null, cancellationToken);
                await mockSearchIndex.Received(1).PartialUpdateObjectsAsync(
                    Arg.Is<IEnumerable<JObject>>(arg => arg.SequenceEqual(dataToUpsert, new JObjectEqualityComparer())), createIfNotExists: true, ct: cancellationToken);
            }
        }


        [TestFixture]
        internal class ProcessCrawlerTasksTests
        {
            private IAlgoliaTaskProcessor algoliaTaskProcessor;
            private readonly IAlgoliaClient mockAlgoliaClient = Substitute.For<IAlgoliaClient>();


            [SetUp]
            public void ProcessCrawlerTasksTestsSetUp()
            {
                mockAlgoliaClient.CrawlUrls(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(args =>
                    Task.FromResult(args.Arg<IEnumerable<string>>().Count()));
                mockAlgoliaClient.DeleteUrls(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(args =>
                    Task.FromResult(args.Arg<IEnumerable<string>>().Count()));

                algoliaTaskProcessor = new DefaultAlgoliaTaskProcessor(mockAlgoliaClient,
                    Substitute.For<IEventLogService>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    Substitute.For<IAlgoliaObjectGenerator>());
            }


            [Test]
            public async Task ProcessCrawlerTasks_ValidTasks_ReturnsProcessedCount()
            {
                var cancellationToken = new CancellationToken();
                var createQueueItem = new AlgoliaCrawlerQueueItem(CRAWLER_ID, "https://test1", AlgoliaTaskType.CREATE);
                var updateQueueItem = new AlgoliaCrawlerQueueItem(CRAWLER_ID, "https://test2", AlgoliaTaskType.UPDATE);
                var deleteQueueItem = new AlgoliaCrawlerQueueItem(CRAWLER_ID, "https://test3", AlgoliaTaskType.DELETE);
                var numProcessed = await algoliaTaskProcessor.ProcessCrawlerTasks(new AlgoliaCrawlerQueueItem[] { createQueueItem, updateQueueItem, deleteQueueItem }, cancellationToken);

                Assert.That(numProcessed, Is.EqualTo(3));
                await mockAlgoliaClient.Received(1).CrawlUrls(CRAWLER_ID, Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(new string[] { "https://test1", "https://test2" })), cancellationToken);
                await mockAlgoliaClient.Received(1).DeleteUrls(CRAWLER_ID, Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(new string[] { "https://test3" })), cancellationToken);
            }
        }
    }
}
