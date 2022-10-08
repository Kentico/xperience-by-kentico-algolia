using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Http;
using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Helpers.Caching.Abstractions;
using CMS.MediaLibrary;
using CMS.WorkflowEngine;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Moq;

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaClientTests
    {
        private static Mock<ISearchIndex> GetMockSearchIndex()
        {
            var mockSearchIndex = new Mock<ISearchIndex>();
            mockSearchIndex.Setup(service => service.DeleteObjectsAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>())).Verifiable();
            mockSearchIndex.Setup(service =>service.DeleteObjectsAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
                .Returns((IEnumerable<string> objectIds, RequestOptions requestOptions, CancellationToken ct) =>
                {
                    return Task.FromResult(new BatchIndexingResponse
                    {
                        Responses = new List<BatchResponse>
                        {
                            new BatchResponse
                            {
                                ObjectIDs = objectIds
                            }
                        }
                    });
                }
            );
            mockSearchIndex.Setup(service => service.PartialUpdateObjectsAsync(It.IsAny<IEnumerable<JObject>>(), null, It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                .Returns((IEnumerable<JObject> data, RequestOptions requestOptions, CancellationToken ct, bool createIfNotExists) =>
                {
                    return Task.FromResult(new BatchIndexingResponse
                    {
                        Responses = new List<BatchResponse>
                        {
                            new BatchResponse
                            {
                                ObjectIDs = new string[data.Count()]
                            }
                        }
                    });
                }
            );

            return mockSearchIndex;
        }


        [TestFixture]
        internal class DeletetRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private IAlgoliaObjectGenerator algoliaObjectGenerator;
            private readonly Mock<ISearchIndex> mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void DeleteRecordsTestsSetUp()
            {
                var mockEventLogService = new MockEventLogService();
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex.Object);

                algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                    mockEventLogService,
                    Substitute.For<IMediaFileInfoProvider>(),
                    Substitute.For<IMediaFileUrlRetriever>());
                algoliaClient = new DefaultAlgoliaClient(mockIndexService,
                    algoliaObjectGenerator,
                    Substitute.For<ICacheAccessor>(),
                    mockEventLogService,
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>());
            }


            [Test]
            public async Task DeleteRecords_ValidIndex_ReturnsProcessedCount()
            {
                var enQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var objectIdEn = algoliaObjectGenerator.GetTreeNodeData(enQueueItem).Value<string>("objectID");
                var czQueueItem = new AlgoliaQueueItem( FakeNodes.ArticleCz, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var objectIdCz = algoliaObjectGenerator.GetTreeNodeData(czQueueItem).Value<string>("objectID");
                var numProcessed = await algoliaClient.DeleteRecords(new string[] { objectIdEn, objectIdCz }, nameof(ArticleEnSearchModel), CancellationToken.None);

                Assert.That(numProcessed, Is.EqualTo(2));
                mockSearchIndex.Verify(service => service.DeleteObjectsAsync( new string[] { objectIdEn, objectIdCz }, null, It.IsAny<CancellationToken>()), Times.Once);
            }
        }


        [TestFixture]
        internal class ProcessAlgoliaTasksTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly Mock<ISearchIndex> mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void ProcessAlgoliaTasksTestsSetUp()
            {
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex.Object);
                algoliaClient = new DefaultAlgoliaClient(mockIndexService,
                    Substitute.For<IAlgoliaObjectGenerator>(),
                    Substitute.For<ICacheAccessor>(),
                    new MockEventLogService(),
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>());
            }


            [Test]
            public async Task ProcessAlgoliaTasks_ValidTasks_ReturnsProcessedCount()
            {
                var queueItems = new List<AlgoliaQueueItem>
                {
                    new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel)),
                    new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel)),
                    new AlgoliaQueueItem(FakeNodes.ProductEn, AlgoliaTaskType.UPDATE, nameof(ProductsSearchModel), new string[] { "DocumentName" })
                };
                var numProcessed = await algoliaClient.ProcessAlgoliaTasks(queueItems, CancellationToken.None);

                Assert.That(numProcessed, Is.EqualTo(3));
            }
        }


        [TestFixture]
        internal class UpsertRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private IAlgoliaObjectGenerator algoliaObjectGenerator;
            private readonly Mock<ISearchIndex> mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void UpsertRecordsTestsSetUp()
            {
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex.Object);

                var mockEventLogService = new MockEventLogService();

                algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                    mockEventLogService,
                    Substitute.For<IMediaFileInfoProvider>(),
                    Substitute.For<IMediaFileUrlRetriever>());
                algoliaClient = new DefaultAlgoliaClient(mockIndexService,
                    algoliaObjectGenerator,
                    Substitute.For<ICacheAccessor>(),
                    mockEventLogService,
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    Substitute.For<IProgressiveCache>(),
                    Substitute.For<ISearchClient>());
            }


            [Test]
            public async Task UpsertRecords_ValidIndex_ReturnsProcessedCount()
            {
                var enQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var dataEn = algoliaObjectGenerator.GetTreeNodeData(enQueueItem);
                var czQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var dataCz = algoliaObjectGenerator.GetTreeNodeData(czQueueItem);
                var numProcessed = await algoliaClient.UpsertRecords(new JObject[] { dataEn, dataCz }, nameof(ArticleEnSearchModel), CancellationToken.None);

                Assert.That(numProcessed, Is.EqualTo(2));
            }
        }
    }
}
