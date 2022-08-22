using Algolia.Search.Clients;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Xperience.AlgoliaSearch;
using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Configuration;
using System.Runtime.CompilerServices;

[assembly: AssemblyDiscoverable]
[assembly: InternalsVisibleTo("Kentico.Xperience.AlgoliaSearch.Tests")]
[assembly: RegisterModule(typeof(AlgoliaSearchModule))]
namespace Kentico.Xperience.AlgoliaSearch
{
    /// <summary>
    /// Initializes the Algolia integration by scanning assemblies for custom models containing the
    /// <see cref="RegisterAlgoliaIndexAttribute"/> and stores them in <see cref="IAlgoliaRegistrationService.RegisteredIndexes"/>.
    /// Also registers event handlers required for indexing content.
    /// </summary>
    public class AlgoliaSearchModule : CMS.DataEngine.Module
    {
        private IAlgoliaIndexingService algoliaIndexingService;
        private IAlgoliaRegistrationService algoliaRegistrationService;
        private IAlgoliaSearchService algoliaSearchService;


        public AlgoliaSearchModule() : base(nameof(AlgoliaSearchModule))
        {
        }


        protected override void OnPreInit()
        {
            base.OnPreInit();

            // Register ISearchClient for CMS application
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                var applicationId = ValidationHelper.GetString(ConfigurationManager.AppSettings["AlgoliaApplicationId"], String.Empty);
                var apiKey = ValidationHelper.GetString(ConfigurationManager.AppSettings["AlgoliaApiKey"], String.Empty);
                if (String.IsNullOrEmpty(applicationId) || String.IsNullOrEmpty(apiKey))
                {
                    // Algolia configuration is not valid, but IEventLogService can't be resolved during OnPreInit.
                    // Set dummy values so that DI is not broken, but errors are still logged later in the initialization
                    applicationId = "NO_APP";
                    apiKey = "NO_KEY";
                }

                var client = new SearchClient(applicationId, apiKey);
                Service.Use<ISearchClient>(client);
            }
        }


        /// <summary>
        /// Registers all Algolia indexes, initializes page event handlers, and ensures the thread
        /// queue worker for processing Algolia tasks.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            algoliaIndexingService = Service.Resolve<IAlgoliaIndexingService>();
            algoliaRegistrationService = Service.Resolve<IAlgoliaRegistrationService>();
            algoliaSearchService = Service.Resolve<IAlgoliaSearchService>();
            algoliaRegistrationService.RegisterAlgoliaIndexes();

            DocumentEvents.Update.Before += LogTreeNodeUpdate;
            DocumentEvents.Insert.After += LogTreeNodeInsert;
            DocumentEvents.Delete.After += LogTreeNodeDelete;
            RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaQueueWorker.Current.EnsureRunningThread();
        }


        /// <summary>
        /// Called after a page is deleted. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogTreeNodeDelete(object sender, DocumentEventArgs e)
        {
            if (EventShouldCancel(e.Node, true))
            {
                return;
            }

            algoliaIndexingService.EnqueueAlgoliaItems(e.Node, true, false);
        }


        /// <summary>
        /// Called after a page is created. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogTreeNodeInsert(object sender, DocumentEventArgs e)
        {
            if (EventShouldCancel(e.Node, false))
            {
                return;
            }

            algoliaIndexingService.EnqueueAlgoliaItems(e.Node, false, true);
        }


        /// <summary>
        /// Called before a page is updated. Logs an Algolia task to be processed later.
        /// </summary>
        private void LogTreeNodeUpdate(object sender, DocumentEventArgs e)
        {
            if (EventShouldCancel(e.Node, false))
            {
                return;
            }

            algoliaIndexingService.EnqueueAlgoliaItems(e.Node, false, false);
        }


        /// <summary>
        /// Returns true if the page event event handler should stop processing. Checks
        /// if the page is indexed by any Algolia index, and for new/updated pages, the
        /// page must be published.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> that triggered the event.</param>
        /// <param name="wasDeleted">True if the <paramref name="node"/> was deleted.</param>
        /// <returns></returns>
        private bool EventShouldCancel(TreeNode node, bool wasDeleted)
        {
            return !algoliaSearchService.IsIndexingEnabled() ||
                !algoliaRegistrationService.IsNodeAlgoliaIndexed(node) ||
                (!wasDeleted && !node.PublishedVersionExists);
        }
    }
}