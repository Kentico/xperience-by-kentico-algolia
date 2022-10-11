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
            private readonly IAlgoliaClient algoliaClient = Substitute.For<IAlgoliaClient>();


            protected override void RegisterTestServices()
            {
                Service.Use<IAlgoliaClient>(algoliaClient);
            }


            [Test]
            public void EnqueueAlgoliaQueueItem_InvalidIndex_ThrowsException_DoesntQueue()
            {
                Assert.Multiple(() => {
                    Assert.Throws<InvalidOperationException>(() => AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(
                        new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, "FAKE_INDEX")));
                    Assert.That(AlgoliaQueueWorker.Current.ItemsInQueue, Is.EqualTo(0));
                });
            }


            [Test]
            public async Task EnqueueAlgoliaQueueItem_ValidItems_ProcessesItems()
            {
                var createTask = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var deleteTask = new AlgoliaQueueItem(FakeNodes.ProductEn, AlgoliaTaskType.DELETE, nameof(ProductsSearchModel));
                AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(createTask);
                AlgoliaQueueWorker.Current.EnqueueAlgoliaQueueItem(deleteTask);

                await algoliaClient.Received(1).ProcessAlgoliaTasks(
                    Arg.Is<IEnumerable<AlgoliaQueueItem>>(arg => arg.SequenceEqual(new AlgoliaQueueItem[] { createTask })), Arg.Any<CancellationToken>());
                await algoliaClient.Received(1).ProcessAlgoliaTasks(
                    Arg.Is<IEnumerable<AlgoliaQueueItem>>(arg => arg.SequenceEqual(new AlgoliaQueueItem[] { deleteTask })), Arg.Any<CancellationToken>());
            }
        }
    }
}
