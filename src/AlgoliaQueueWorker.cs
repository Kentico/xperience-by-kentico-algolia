using System;
using System.Collections.Generic;
using System.Threading;

using CMS.Base;
using CMS.Core;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Thread worker which enqueues recently updated or deleted nodes indexed
    /// by Algolia and processes the tasks in the background thread.
    /// </summary>
    internal class AlgoliaQueueWorker : ThreadQueueWorker<AlgoliaQueueItem, AlgoliaQueueWorker>
    {
        private readonly IAlgoliaClient algoliaClient;


        /// <inheritdoc />
        protected override int DefaultInterval => 10000;


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaQueueItem"/> class. Should
        /// not be called directly- the worker should be initialized during startup using
        /// <see cref="ThreadWorker{T}.EnsureRunningThread"/>.
        /// </summary>
        public AlgoliaQueueWorker()
        {
            algoliaClient = Service.Resolve<IAlgoliaClient>();
        }


        /// <summary>
        /// Adds an <see cref="AlgoliaQueueItem"/> to the worker queue to be processed.
        /// </summary>
        /// <param name="queueItem">The item to be added to the queue.</param>
        /// <exception cref="InvalidOperationException" />
        public void EnqueueAlgoliaQueueItem(AlgoliaQueueItem queueItem)
        {
            if (queueItem == null || queueItem.Node == null || String.IsNullOrEmpty(queueItem.IndexName))
            {
                return;
            }

            if (queueItem.TaskType == AlgoliaTaskType.UNKNOWN)
            {
                return;
            }

            if (IndexStore.Instance.Get(queueItem.IndexName) == null)
            {
                throw new InvalidOperationException($"Attempted to log task for Algolia index '{queueItem.IndexName},' but it is not registered.");
            }

            Current.Enqueue(queueItem, false);
        }


        /// <inheritdoc />
        protected override void Finish()
        {
            RunProcess();
        }


        /// <inheritdoc />
        protected override int ProcessItems(IEnumerable<AlgoliaQueueItem> items)
        {
            return algoliaClient.ProcessAlgoliaTasks(items, CancellationToken.None).Result;
        }


        /// <inheritdoc/>
        protected override void ProcessItem(AlgoliaQueueItem item)
        {
        }
    }
}