using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using static Kentico.Xperience.Algolia.Models.AlgoliaQueueItem;

[assembly: RegisterImplementation(typeof(IAlgoliaClient), typeof(DefaultAlgoliaClient), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaClient"/>.
    /// </summary>
    internal class DefaultAlgoliaClient : IAlgoliaClient
    {
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly IConversionService conversionService;
        private readonly IEventLogService eventLogService;
        private readonly IMediaFileInfoProvider mediaFileInfoProvider;
        private readonly IMediaFileUrlRetriever mediaFileUrlRetriever;
        private readonly string[] ignoredPropertiesForTrackingChanges = new string[] {
            nameof(AlgoliaSearchModel.ObjectID),
            nameof(AlgoliaSearchModel.Url),
            nameof(AlgoliaSearchModel.ClassName)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IConversionService conversionService,
            IEventLogService eventLogService,
            IMediaFileInfoProvider mediaFileInfoProvider,
            IMediaFileUrlRetriever mediaFileUrlRetriever)
        {
            this.eventLogService = eventLogService;
            this.algoliaIndexService = algoliaIndexService;
            this.conversionService = conversionService;
            this.mediaFileInfoProvider = mediaFileInfoProvider;
            this.mediaFileUrlRetriever = mediaFileUrlRetriever;
        }


        public Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (objectIds == null || !objectIds.Any())
            {
                return Task.FromResult(0);
            }

            return DeleteRecordsInternal(objectIds, indexName);
        }


        public async Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> items)
        {
            var successfulOperations = 0;

            // Group queue items based on index name
            var groups = items.GroupBy(item => item.IndexName);
            foreach (var group in groups)
            {
                try
                {
                    var algoliaIndex = IndexStore.Instance.Get(group.Key);
                    if (algoliaIndex == null)
                    {
                        eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), $"Attempted to process tasks for index '{group.Key},' but the index is not registered.");
                        continue;
                    }

                    var deleteTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.DELETE);
                    var updateTasks = group.Where(queueItem => queueItem.TaskType == AlgoliaTaskType.UPDATE || queueItem.TaskType == AlgoliaTaskType.CREATE);
                    var upsertData = updateTasks.Select(queueItem => GetTreeNodeData(queueItem.Node, algoliaIndex.Type, queueItem.TaskType));
                    var deleteData = deleteTasks.Select(queueItem => queueItem.Node.DocumentID.ToString());

                    successfulOperations += await UpsertRecords(upsertData, group.Key);
                    successfulOperations += await DeleteRecords(deleteData, group.Key);
                }
                catch (Exception ex)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), ex.Message);
                }
            }

            return successfulOperations;
        }


        public Task Rebuild(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = IndexStore.Instance.Get(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"The index '{indexName}' is not registered.");
            }

            return RebuildInternal(algoliaIndex);
        }


        public Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (dataObjects == null || !dataObjects.Any())
            {
                return Task.FromResult(0);
            }

            return UpsertRecordsInternal(dataObjects, indexName);
        }


        private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string indexName)
        {
            var deletedCount = 0;
            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var batchIndexingResponse = await searchIndex.DeleteObjectsAsync(objectIds).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }


        /// <summary>
        /// Converts the assets from the <paramref name="node"/>'s value into absolute URLs.
        /// </summary>
        /// <remarks>Logs an error if the definition of the <paramref name="columnName"/> can't
        /// be found.</remarks>
        /// <param name="node">The <see cref="TreeNode"/> the value was loaded from.</param>
        /// <param name="nodeValue">The original value of the column.</param>
        /// <param name="columnName">The name of the column the value was loaded from.</param>
        /// <returns>An list of absolute URLs, or an empty list.</returns>
        private IEnumerable<string> GetAssetUrlsForColumn(TreeNode node, object nodeValue, string columnName)
        {
            var strValue = conversionService.GetString(nodeValue, String.Empty);
            if (String.IsNullOrEmpty(strValue))
            {
                return Enumerable.Empty<string>();
            }

            // Ensure field is Asset type
            var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(node.ClassName, false);
            var formInfo = new FormInfo(dataClassInfo.ClassFormDefinition);
            var field = formInfo.GetFormField(columnName);
            if (field == null)
            {
                eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(GetAssetUrlsForColumn), $"Unable to load field definition for page type '{node.ClassName}' column name '{columnName}.'");
                return Enumerable.Empty<string>();
            }

            if (!field.DataType.Equals(FieldDataType.Assets, StringComparison.OrdinalIgnoreCase))
            {
                return Enumerable.Empty<string>();
            }

            var assets = JsonConvert.DeserializeObject<IEnumerable<AssetRelatedItem>>(strValue);
            var mediaFiles = mediaFileInfoProvider.Get().ForAssets(assets);

            return mediaFiles.Select(file => mediaFileUrlRetriever.Retrieve(file).RelativePath);
        }


        private string[] GetIndexedColumnNames(Type searchModel)
        {
            // Don't include properties with SourceAttribute at first, check the sources and add to list after
            var indexedColumnNames = searchModel.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !Attribute.IsDefined(prop, typeof(SourceAttribute))).Select(prop => prop.Name).ToList();
            var propertiesWithSourceAttribute = searchModel.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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


        /// <summary>
        /// Gets the <paramref name="node"/> value using the <paramref name="property"/>
        /// name, or the property's <see cref="SourceAttribute"/> if specified.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> to load a value from.</param>
        /// <param name="property">The Algolia search model property.</param>
        /// <param name="searchModelType">The Algolia search model.</param>
        private object GetNodeValue(TreeNode node, PropertyInfo property, Type searchModelType)
        {
            var usedColumn = property.Name;
            var nodeValue = node.GetValue(property.Name);
            var searchModel = Activator.CreateInstance(searchModelType) as AlgoliaSearchModel;
            if (Attribute.IsDefined(property, typeof(SourceAttribute)))
            {
                // Property uses SourceAttribute, loop through column names until a non-null value is found
                var sourceAttribute = property.GetCustomAttributes<SourceAttribute>(false).FirstOrDefault();
                foreach (var source in sourceAttribute.Sources)
                {
                    nodeValue = node.GetValue(source);
                    if (nodeValue != null)
                    {
                        usedColumn = source;
                        break;
                    }
                }
            }

            // Convert node value to URLs if necessary
            if (Attribute.IsDefined(property, typeof(MediaUrlsAttribute)))
            {
                nodeValue = GetAssetUrlsForColumn(node, nodeValue, usedColumn);
            }

            nodeValue = searchModel.OnIndexingProperty(node, property.Name, usedColumn, nodeValue);

            return nodeValue;
        }


        /// <summary>
        /// Creates an anonymous object with the indexed column names of the <paramref name="searchModelType"/> and
        /// their values loaded from the passed <paramref name="node"/>.
        /// </summary>
        /// <remarks>When the <paramref name="taskType"/> is <see cref="AlgoliaTaskType.UPDATE"/>, only the updated
        /// columns will be included in the resulting object for a partial update. For <see cref="AlgoliaTaskType.CREATE"/>,
        /// all indexed columns are included.</remarks>
        /// <param name="node">The <see cref="TreeNode"/> to load values from.</param>
        /// <param name="searchModelType">The class of the Algolia search model.</param>
        /// <param name="taskType">The Algolia task being processed.</param>
        /// <returns>The anonymous data that will be passed to Algolia.</returns>
        /// <exception cref="ArgumentNullException" />
        private JObject GetTreeNodeData(TreeNode node, Type searchModelType, AlgoliaTaskType taskType)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (searchModelType == null)
            {
                throw new ArgumentNullException(nameof(searchModelType));
            }

            var data = new JObject();
            MapChangedProperties(node, data, searchModelType, taskType);
            MapCommonProperties(node, data);

            return data;
        }


        private void MapChangedProperties(TreeNode node, JObject data, Type searchModelType, AlgoliaTaskType taskType)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new DecimalPrecisionConverter());

            var columnsToUpdate = new List<string>();
            var indexedColumns = GetIndexedColumnNames(searchModelType);
            if (taskType == AlgoliaTaskType.CREATE)
            {
                columnsToUpdate.AddRange(indexedColumns);
            }
            else if (taskType == AlgoliaTaskType.UPDATE)
            {
                columnsToUpdate.AddRange(node.ChangedColumns().Intersect(indexedColumns));
            }

            var searchModel = Activator.CreateInstance(searchModelType);
            var properties = searchModel.GetType().GetProperties().Where(prop => columnsToUpdate.Contains(prop.Name));
            foreach (var prop in properties)
            {
                object nodeValue = GetNodeValue(node, prop, searchModelType);
                if (nodeValue == null)
                {
                    continue;
                }

                data.Add(prop.Name, JToken.FromObject(nodeValue, serializer));
            }
        }


        /// <summary>
        /// Sets values in the <paramref name="data"/> object using the common search model properties
        /// located within the <see cref="AlgoliaSearchModel"/> class.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> to load values from.</param>
        /// <param name="data">The dynamic data that will be passed to Algolia.</param>
        private void MapCommonProperties(TreeNode node, JObject data)
        {
            data["objectID"] = node.DocumentID.ToString();
            data[nameof(AlgoliaSearchModel.ClassName)] = node.ClassName;

            try
            {
                data[nameof(AlgoliaSearchModel.Url)] = DocumentURLProvider.GetAbsoluteUrl(node);
            }
            catch (Exception)
            {
                // GetAbsoluteUrl can throw an exception when processing a page update AlgoliaQueueItem
                // and the page was deleted before the update task has processed. In this case, upsert an
                // empty URL
                data[nameof(AlgoliaSearchModel.Url)] = String.Empty;
            }
        }


        private async Task RebuildInternal(AlgoliaIndex algoliaIndex)
        {
            var indexedNodes = new List<TreeNode>();
            var includedPathAttributes = algoliaIndex.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            foreach (var includedPathAttribute in includedPathAttributes)
            {
                var query = new MultiDocumentQuery()
                    .Path(includedPathAttribute.AliasPath)
                    .PublishedVersion()
                    .WithCoupledColumns();

                if (includedPathAttribute.PageTypes.Length > 0)
                {
                    query.Types(includedPathAttribute.PageTypes);
                }

                indexedNodes.AddRange(query.TypedResult);
            }

            var data = indexedNodes.Select(node => GetTreeNodeData(node, algoliaIndex.Type, AlgoliaTaskType.CREATE));
            var searchIndex = algoliaIndexService.InitializeIndex(algoliaIndex.IndexName);
            await searchIndex.ReplaceAllObjectsAsync(data).ConfigureAwait(false);
        }


        private async Task<int> UpsertRecordsInternal(IEnumerable<JObject> dataObjects, string indexName)
        {
            var upsertedCount = 0;
            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var batchIndexingResponse = await searchIndex.PartialUpdateObjectsAsync(dataObjects, createIfNotExists: true).ConfigureAwait(false);
            foreach (var response in batchIndexingResponse.Responses)
            {
                upsertedCount += response.ObjectIDs.Count();
            }

            return upsertedCount;
        }
    }
}
