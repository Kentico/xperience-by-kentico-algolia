using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

using CMS.Core;

using Kentico.Xperience.Algolia.Services;

using NSubstitute;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class AlgoliaCrawlerQueueWorkerTests
    {
        [TestFixture]
        internal class EnqueueAlgoliaQueueItemTests : AlgoliaTests
        {
            private readonly IAlgoliaTaskProcessor algoliaTaskProcessor = Substitute.For<IAlgoliaTaskProcessor>();


            protected override void RegisterTestServices()
            {
                Service.Use<IAlgoliaTaskProcessor>(algoliaTaskProcessor);
            }


            [Test]
            public async Task EnqueueCrawlerQueueItem_ValidItems_ProcessesItems()
            {
                var createTask = new AlgoliaCrawlerQueueItem(CRAWLER_ID, "https://test", AlgoliaTaskType.CREATE);
                var deleteTask = new AlgoliaCrawlerQueueItem(CRAWLER_ID, "https://test", AlgoliaTaskType.DELETE);
                AlgoliaCrawlerQueueWorker.EnqueueCrawlerQueueItem(createTask);
                AlgoliaCrawlerQueueWorker.EnqueueCrawlerQueueItem(deleteTask);

                await algoliaTaskProcessor.Received(1).ProcessCrawlerTasks(
                    Arg.Is<IEnumerable<AlgoliaCrawlerQueueItem>>(arg => arg.SequenceEqual(new AlgoliaCrawlerQueueItem[] { createTask })), Arg.Any<CancellationToken>());
                await algoliaTaskProcessor.Received(1).ProcessCrawlerTasks(
                    Arg.Is<IEnumerable<AlgoliaCrawlerQueueItem>>(arg => arg.SequenceEqual(new AlgoliaCrawlerQueueItem[] { deleteTask })), Arg.Any<CancellationToken>());
            }
        }
    }
}
