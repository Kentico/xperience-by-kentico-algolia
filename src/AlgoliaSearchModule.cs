using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Services;

namespace Kentico.Xperience.Algolia
{
    /// <summary>
    /// Initializes page event handlers, and ensures the thread queue worker for processing Algolia tasks.
    /// </summary>
    internal class AlgoliaSearchModule : Module
    {
        private IAlgoliaTaskLogger algoliaTaskLogger;
        private IAppSettingsService appSettingsService;
        private IConversionService conversionService;
        private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "AlgoliaSearchDisableIndexing";


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
            WorkflowEvents.Archive.After += HandleWorkflowEvent;
            RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaQueueWorker.Current.EnsureRunningThread();
        }


        /// <summary>
        /// Returns <c>true</c> if the event event handler should continue processing and log
        /// an Algolia task.
        /// </summary>
        private bool EventShouldContinue(TreeNode node)
        {
            return !conversionService.GetBoolean(appSettingsService[APP_SETTINGS_KEY_INDEXING_DISABLED], false) &&
                node.IsAlgoliaIndexed();
        }


        /// <summary>
        /// Called after a page is published or archived. Logs an Algolia task to be processed later.
        /// </summary>
        private void HandleWorkflowEvent(object sender, WorkflowEventArgs e)
        {
            if (!EventShouldContinue(e.Document))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, e.CurrentHandler.Name);
        }


        /// <summary>
        /// Called before a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void HandleDocumentEvent(object sender, DocumentEventArgs e)
        {
            if (!EventShouldContinue(e.Node))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Node, e.CurrentHandler.Name);
        }
    }
}