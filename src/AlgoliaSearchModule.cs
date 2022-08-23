using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.AlgoliaSearch;
using Kentico.Xperience.AlgoliaSearch.Services;

using System.Runtime.CompilerServices;

[assembly: AssemblyDiscoverable]
[assembly: InternalsVisibleTo("Kentico.Xperience.AlgoliaSearch.Tests")]
[assembly: RegisterModule(typeof(AlgoliaSearchModule))]
namespace Kentico.Xperience.AlgoliaSearch
{
    /// <summary>
    /// Initializes page event handlers, and ensures the thread queue worker for processing Algolia tasks.
    /// </summary>
    public class AlgoliaSearchModule : Module
    {
        private IAlgoliaHelper algoliaHelper;
        private IAlgoliaTaskLogger algoliaTaskLogger;


        public AlgoliaSearchModule() : base(nameof(AlgoliaSearchModule))
        {
        }


        protected override void OnInit()
        {
            base.OnInit();

            algoliaHelper = Service.Resolve<IAlgoliaHelper>();
            algoliaTaskLogger = Service.Resolve<IAlgoliaTaskLogger>();

            DocumentEvents.Delete.After += LogTreeNodeDelete;
            WorkflowEvents.Publish.After += LogTreeNodePublish;
            WorkflowEvents.Archive.After += LogTreeNodeArchive;
            RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaQueueWorker.Current.EnsureRunningThread();
        }


        /// <summary>
        /// Returns <c>true</c> if the event event handler should continue processing and log
        /// an Algolia task.
        /// </summary>
        private bool EventShouldContinue(TreeNode node)
        {
            return algoliaHelper.IsIndexingEnabled() &&
                algoliaHelper.IsNodeAlgoliaIndexed(node);
        }


        /// <summary>
        /// Called after a page is archived. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogTreeNodeArchive(object sender, WorkflowEventArgs e)
        {
            if (!EventShouldContinue(e.Document))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, WorkflowEvents.Archive.Name);
        }


        /// <summary>
        /// Called after a page is published manually or by content scheduling. Logs an Algolia
        /// task to be processed later.
        /// </summary>
        private void LogTreeNodePublish(object sender, WorkflowEventArgs e)
        {
            if (!EventShouldContinue(e.Document))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, WorkflowEvents.Publish.Name);
        }


        /// <summary>
        /// Called after a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogTreeNodeDelete(object sender, DocumentEventArgs e)
        {
            if (!EventShouldContinue(e.Node))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Node, DocumentEvents.Delete.Name);
        }
    }
}