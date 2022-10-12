using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Services;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Initializes page event handlers, and ensures the thread queue workers for processing Algolia tasks.
    /// </summary>
    internal class AlgoliaSearchModule : Module
    {
        private IAlgoliaTaskLogger algoliaTaskLogger;
        private IAppSettingsService appSettingsService;
        private IConversionService conversionService;
        private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "AlgoliaSearchDisableIndexing";


        private bool IndexingDisabled
        {
            get
            {
                return conversionService.GetBoolean(appSettingsService[APP_SETTINGS_KEY_INDEXING_DISABLED], false);
            }
        }


        /// <inheritdoc/>
        public AlgoliaSearchModule() : base(nameof(AlgoliaSearchModule))
        {
        }


        /// <inheritdoc/>
        protected override void OnInit()
        {
            base.OnInit();

            algoliaTaskLogger = Service.Resolve<IAlgoliaTaskLogger>();
            appSettingsService = Service.Resolve<IAppSettingsService>();
            conversionService = Service.Resolve<IConversionService>();

            DocumentEvents.Delete.Before += HandleDocumentEvent;
            WorkflowEvents.Publish.After += HandleWorkflowEvent;
            WorkflowEvents.Archive.Before += HandleWorkflowEvent;
            RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaQueueWorker.Current.EnsureRunningThread();
            RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaCrawlerQueueWorker.Current.EnsureRunningThread();
        }


        /// <summary>
        /// Called when a page is published or archived. Logs an Algolia task to be processed later.
        /// </summary>
        private void HandleWorkflowEvent(object sender, WorkflowEventArgs e)
        {
            if (IndexingDisabled)
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, e.CurrentHandler.Name);
        }


        /// <summary>
        /// Called when a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void HandleDocumentEvent(object sender, DocumentEventArgs e)
        {
            if (IndexingDisabled)
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Node, e.CurrentHandler.Name);
        }
    }
}