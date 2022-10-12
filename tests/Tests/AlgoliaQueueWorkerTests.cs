using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.Core;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class AlgoliaQueueWorkerTests
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
            public async Task EnqueueAlgoliaQueueItem_ValidItems_ProcessesItems()
            {
                var createTask = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var deleteTask = new AlgoliaQueueItem(FakeNodes.ProductEn, AlgoliaTaskType.DELETE, nameof(ProductsSearchModel));
                AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(createTask);
                AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(deleteTask);

                await algoliaTaskProcessor.Received(1).ProcessAlgoliaTasks(
                    Arg.Is<IEnumerable<AlgoliaQueueItem>>(arg => arg.SequenceEqual(new AlgoliaQueueItem[] { createTask })), Arg.Any<CancellationToken>());
                await algoliaTaskProcessor.Received(1).ProcessAlgoliaTasks(
                    Arg.Is<IEnumerable<AlgoliaQueueItem>>(arg => arg.SequenceEqual(new AlgoliaQueueItem[] { deleteTask })), Arg.Any<CancellationToken>());
            }
        }
    }
}
