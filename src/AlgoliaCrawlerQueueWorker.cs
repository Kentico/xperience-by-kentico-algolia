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
    internal class AlgoliaCrawlerQueueWorker : ThreadQueueWorker<AlgoliaCrawlerQueueItem, AlgoliaCrawlerQueueWorker>
    {
        private readonly IAlgoliaTaskProcessor algoliaTaskProcessor;


        /// <inheritdoc/>
        protected override int DefaultInterval => 10000;


        public AlgoliaCrawlerQueueWorker()
        {
            algoliaTaskProcessor = Service.Resolve<IAlgoliaTaskProcessor>();
        }


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
