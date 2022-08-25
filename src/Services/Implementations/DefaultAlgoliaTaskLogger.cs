using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;

using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Extensions;
using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaTaskLogger"/>.
    /// </summary>
    internal class DefaultAlgoliaTaskLogger : IAlgoliaTaskLogger
    {
        private readonly IEventLogService eventLogService;
        private readonly string[] ignoredPropertiesForTrackingChanges = new string[] {
            nameof(AlgoliaSearchModel.ObjectID),
            nameof(AlgoliaSearchModel.Url),
            nameof(AlgoliaSearchModel.ClassName)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaTaskLogger"/> class.
        /// </summary>
        public DefaultAlgoliaTaskLogger(IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
        }


        public void LogTask(AlgoliaQueueItem task)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItem(task);
        }


        public void LogTasks(IEnumerable<AlgoliaQueueItem> tasks)
        {
            AlgoliaQueueWorker.EnqueueAlgoliaQueueItems(tasks);
        }


        public void HandleEvent(TreeNode node, string eventName)
        {
            foreach (var indexName in IndexStore.Instance.GetAll().Select(index => index.IndexName))
            {
                if (!node.IsIndexedByIndex(indexName))
                {
                    continue;
                }

                var indexedColumns = GetIndexedColumnNames(indexName);
                if (indexedColumns.Length == 0)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaTaskLogger), nameof(LogTasks), $"Unable to enqueue node change: Error loading indexed columns.");
                    continue;
                }

                if (eventName.Equals(WorkflowEvents.Publish.Name) &&
                    node.WorkflowHistory.Count > 0 &&
                    !node.AnyItemChanged(indexedColumns))
                {
                    // For Publish event, don't update Algolia if nothing changed. Only applies if it's not the first publishing
                    continue;
                }

                var shouldDelete = eventName.Equals(DocumentCultureDataInfo.TYPEINFO.Events.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
                    eventName.Equals(DocumentCultureDataInfo.TYPEINFO.Events.BulkDelete.Name, StringComparison.OrdinalIgnoreCase) ||
                    eventName.Equals(WorkflowEvents.Archive.Name, StringComparison.OrdinalIgnoreCase);

                LogTask(new AlgoliaQueueItem()
                {
                    Node = node,
                    Delete = shouldDelete,
                    IndexName = indexName
                });
            }
        }


        private string[] GetIndexedColumnNames(string indexName)
        {
            var alogliaIndex = IndexStore.Instance.Get(indexName);
            if (alogliaIndex == null)
            {
                return new string[0];
            }

            // Don't include properties with SourceAttribute at first, check the sources and add to list after
            var indexedColumnNames = alogliaIndex.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !Attribute.IsDefined(prop, typeof(SourceAttribute))).Select(prop => prop.Name).ToList();
            var propertiesWithSourceAttribute = alogliaIndex.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(prop => Attribute.IsDefined(prop, typeof(SourceAttribute)));
            foreach (var property in propertiesWithSourceAttribute)
            {
                var sourceAttribute = property.GetCustomAttributes<SourceAttribute>(false).FirstOrDefault();
                if (sourceAttribute == null)
                {
                    continue;
                }

                indexedColumnNames.AddRange(sourceAttribute.Sources);
            }

            // Remove column names from AlgoliaSearchModel that aren't database columns
            indexedColumnNames.RemoveAll(col => ignoredPropertiesForTrackingChanges.Contains(col));

            return indexedColumnNames.ToArray();
        }
    }
}
