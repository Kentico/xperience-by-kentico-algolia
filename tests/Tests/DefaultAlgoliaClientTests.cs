using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
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

using Newtonsoft.Json.Linq;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaClientTests
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
        internal class DeletetRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private IAlgoliaObjectGenerator algoliaObjectGenerator;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void DeleteRecordsTestsSetUp()
            {
                var mockEventLogService = new MockEventLogService();
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);

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
                var objectIds = new string[] { objectIdEn, objectIdCz };
                var numProcessed = await algoliaClient.DeleteRecords(objectIds, nameof(ArticleEnSearchModel), CancellationToken.None);

                Assert.That(numProcessed, Is.EqualTo(2));
                await mockSearchIndex.Received(1).DeleteObjectsAsync(Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(objectIds)), null, Arg.Any<CancellationToken>());
            }
        }


        [TestFixture]
        internal class GetStatisticsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private readonly IProgressiveCache mockProgressiveCache = Substitute.For<IProgressiveCache>();
            private readonly ISearchClient mockSearchClient = Substitute.For<ISearchClient>();
            

            [SetUp]
            public void ProcessAlgoliaTasksTestsSetUp()
            {
                mockSearchClient.ListIndicesAsync(null, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(args => Task.FromResult(
                    new ListIndicesResponse
                    {
                        Items = new List<IndicesResponse>()
                    }
                ));
                mockProgressiveCache.LoadAsync(Arg.Any<Func<CacheSettings, Task<List<IndicesResponse>>>>(), Arg.Any<CacheSettings>()).ReturnsForAnyArgs(async args =>
                {
                    // Execute the passed function
                    await args.ArgAt<Func<CacheSettings, Task<List<IndicesResponse>>>>(0)(args.ArgAt<CacheSettings>(1));

                    return null;
                });

                algoliaClient = new DefaultAlgoliaClient(Substitute.For<IAlgoliaIndexService>(),
                    Substitute.For<IAlgoliaObjectGenerator>(),
                    Substitute.For<ICacheAccessor>(),
                    new MockEventLogService(),
                    Substitute.For<IVersionHistoryInfoProvider>(),
                    Substitute.For<IWorkflowStepInfoProvider>(),
                    mockProgressiveCache,
                    mockSearchClient);
            }


            [Test]
            public async Task GetStatistics_CallsMethods()
            {
                await algoliaClient.GetStatistics(CancellationToken.None);
                await mockSearchClient.Received(1).ListIndicesAsync(null, Arg.Any<CancellationToken>());
                await mockProgressiveCache.Received(1).LoadAsync(Arg.Any<Func<CacheSettings, Task<List<IndicesResponse>>>>(), Arg.Any<CacheSettings>());
            }
        }


        [TestFixture]
        internal class ProcessAlgoliaTasksTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private IAlgoliaObjectGenerator algoliaObjectGenerator;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void ProcessAlgoliaTasksTestsSetUp()
            {
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);
                algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                    new MockEventLogService(),
                    Substitute.For<IMediaFileInfoProvider>(),
                    Substitute.For<IMediaFileUrlRetriever>());
                algoliaClient = new DefaultAlgoliaClient(mockIndexService,
                    algoliaObjectGenerator,
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
                var createQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var deleteQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.DELETE, nameof(ArticleEnSearchModel));
                var updateQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.UPDATE, nameof(ArticleEnSearchModel), new string[] { "DocumentName" });
                var IdToDelete = algoliaObjectGenerator.GetTreeNodeData(deleteQueueItem).Value<string>("objectID");
                var dataToUpsert = new JObject[] {
                    algoliaObjectGenerator.GetTreeNodeData(createQueueItem),
                    algoliaObjectGenerator.GetTreeNodeData(updateQueueItem)
                };
                var numProcessed = await algoliaClient.ProcessAlgoliaTasks(new AlgoliaQueueItem[] { createQueueItem, updateQueueItem, deleteQueueItem }, CancellationToken.None);
                
                Assert.That(numProcessed, Is.EqualTo(3));
                await mockSearchIndex.Received(1).DeleteObjectsAsync(Arg.Is<IEnumerable<string>>(arg => arg.SequenceEqual(new string[] { IdToDelete })), null, Arg.Any<CancellationToken>());
                await mockSearchIndex.Received(1).PartialUpdateObjectsAsync(
                    Arg.Is<IEnumerable<JObject>>(arg => arg.SequenceEqual(dataToUpsert, new JObjectEqualityComparer())), createIfNotExists: true, ct: Arg.Any<CancellationToken>());
            }
        }


        [TestFixture]
        internal class UpsertRecordsTests : AlgoliaTests
        {
            private IAlgoliaClient algoliaClient;
            private IAlgoliaObjectGenerator algoliaObjectGenerator;
            private readonly ISearchIndex mockSearchIndex = GetMockSearchIndex();


            [SetUp]
            public void UpsertRecordsTestsSetUp()
            {
                var mockIndexService = Substitute.For<IAlgoliaIndexService>();
                mockIndexService.InitializeIndex(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsForAnyArgs(mockSearchIndex);

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
                var czQueueItem = new AlgoliaQueueItem(FakeNodes.ArticleCz, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var dataToUpsert = new JObject[] {
                    algoliaObjectGenerator.GetTreeNodeData(enQueueItem),
                    algoliaObjectGenerator.GetTreeNodeData(czQueueItem)
                };
                var numProcessed = await algoliaClient.UpsertRecords(dataToUpsert, nameof(ArticleEnSearchModel), CancellationToken.None);

                Assert.That(numProcessed, Is.EqualTo(2));
                await mockSearchIndex.Received(1).PartialUpdateObjectsAsync(
                    Arg.Is<IEnumerable<JObject>>(arg => arg.SequenceEqual(dataToUpsert, new JObjectEqualityComparer())), createIfNotExists: true, ct: Arg.Any<CancellationToken>());
            }
        }
    }
}
