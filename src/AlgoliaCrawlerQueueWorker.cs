using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.Core;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Thread worker which enqueues recently updated or deleted nodes and processes
    /// the tasks in the background thread.
    /// </summary>
    internal class AlgoliaCrawlerQueueWorker : ThreadQueueWorker<AlgoliaCrawlerQueueItem, AlgoliaCrawlerQueueWorker>
    {
        private readonly IAlgoliaTaskProcessor algoliaTaskProcessor;


        /// <inheritdoc/>
        protected override int DefaultInterval => 10000;


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaCrawlerQueueWorker"/> class.
        /// Should not be called directly- the worker should be initialized during startup using
        /// <see cref="ThreadWorker{T}.EnsureRunningThread"/>.
        /// </summary>
        public AlgoliaCrawlerQueueWorker()
        {
            algoliaTaskProcessor = Service.Resolve<IAlgoliaTaskProcessor>();
        }


        /// <summary>
        /// Adds an <see cref="AlgoliaCrawlerQueueItem"/> to the worker queue to be processed.
        /// </summary>
        /// <param name="queueItem">The item to be added to the queue.</param>
        /// <exception cref="InvalidOperationException" />
        public void EnqueueCrawlerQueueItem(AlgoliaCrawlerQueueItem queueItem)
        {
            if (queueItem == null || String.IsNullOrEmpty(queueItem.CrawlerId) || String.IsNullOrEmpty(queueItem.Url))
            {
                return;
            }

            if (queueItem.TaskType == AlgoliaTaskType.UNKNOWN)
            {
                return;
            }

            if (!IndexStore.Instance.GetAllCrawlers().Any(id => id.Equals(queueItem.CrawlerId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Attempted to log task for Algolia crawler '{queueItem.CrawlerId},' but it is not registered.");
            }

            Current.Enqueue(queueItem, false);
        }


        /// <inheritdoc/>
        protected override void Finish()
        {
            RunProcess();
        }


        /// <inheritdoc/>
        protected override void ProcessItem(AlgoliaCrawlerQueueItem task)
        {
        }


        /// <inheritdoc/>
        protected override int ProcessItems(IEnumerable<AlgoliaCrawlerQueueItem> tasks)
        {
            return algoliaTaskProcessor.ProcessCrawlerTasks(tasks, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
