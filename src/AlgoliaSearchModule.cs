using System.Runtime.CompilerServices;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

using Kentico.Xperience.Algolia;
using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Services;

[assembly: AssemblyDiscoverable]
[assembly: InternalsVisibleTo("Kentico.Xperience.Algolia.Tests")]
[assembly: RegisterModule(typeof(AlgoliaSearchModule))]
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

            DocumentEvents.Delete.Before += LogDelete;
            WorkflowEvents.Publish.After += LogPublish;
            WorkflowEvents.Archive.After += LogArchive;
            //DocumentCultureDataInfo.TYPEINFO.Events.BulkDelete.Before += LogBulkDelete;
            //DocumentCultureDataInfo.TYPEINFO.Events.Delete.Before += LogDelete;
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
        /// Called after a page is archived. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogArchive(object sender, WorkflowEventArgs e)
        {
            if (!EventShouldContinue(e.Document))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, WorkflowEvents.Archive.Name);
        }


        /// <summary>
        /// Called before a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogDelete(object sender, DocumentEventArgs e)
        {
            if (!EventShouldContinue(e.Node))
            {
                return;
            }

            // TODO: Update event name everywhere if we use this
            algoliaTaskLogger.HandleEvent(e.Node, DocumentCultureDataInfo.TYPEINFO.Events.Delete.Name);
        }


        /*/// <summary>
        /// Called before pages are bulk deleted. Logs Algolia tasks to be processed later.
        /// </summary>
        private void LogBulkDelete(object sender, BulkDeleteEventArgs e)
        {
            var deletedNodeIds = new ObjectQuery<DocumentCultureDataInfo>()
                .Column(nameof(DocumentCultureDataInfo.DocumentNodeID))
                .Where(e.WhereCondition)
                .GetListResult<int>();
            var nodes = new DocumentQuery()
                .WhereIn(nameof(TreeNode.DocumentID), deletedNodeIds)
                .PublishedVersion();
            foreach (var node in nodes)
            {
                if (!EventShouldContinue(node))
                {
                    continue;
                }

                algoliaTaskLogger.HandleEvent(node, DocumentCultureDataInfo.TYPEINFO.Events.BulkDelete.Name);
            }
        }


        /// <summary>
        /// Called before a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogDelete(object sender, ObjectEventArgs e)
        {
            var cultureInfo = e.Object as DocumentCultureDataInfo;
            var nodeData = DocumentNodeDataInfo.Provider.Get(cultureInfo.DocumentNodeID);
            var node = new DocumentQuery()
                .TopN(1)
                .WhereEquals(nameof(TreeNode.DocumentID), cultureInfo.DocumentID)
                .PublishedVersion()
                .FirstOrDefault();
            if (!EventShouldContinue(node))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(node, DocumentCultureDataInfo.TYPEINFO.Events.Delete.Name);
        }*/


        /// <summary>
        /// Called after a page is published. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogPublish(object sender, WorkflowEventArgs e)
        {
            if (!EventShouldContinue(e.Document))
            {
                return;
            }

            algoliaTaskLogger.HandleEvent(e.Document, WorkflowEvents.Publish.Name);
        }
    }
}