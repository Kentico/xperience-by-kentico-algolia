using Algolia.Search.Clients;
using Algolia.Search.Models.Common;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: RegisterImplementation(typeof(IAlgoliaClient), typeof(DefaultAlgoliaClient), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaClient"/>.
    /// </summary>
    internal class DefaultAlgoliaClient : IAlgoliaClient
    {
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly IAlgoliaIndexStore algoliaIndexStore;
        private readonly IAlgoliaTaskLogger algoliaTaskLogger;
        private readonly IEventLogService eventLogService;
        private readonly IMediaFileInfoProvider mediaFileInfoProvider;
        private readonly IMediaFileUrlRetriever mediaFileUrlRetriever;
        private readonly ISearchClient searchClient;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaClient"/> class.
        /// </summary>
        public DefaultAlgoliaClient(IAlgoliaIndexService algoliaIndexService,
            IAlgoliaIndexStore algoliaIndexStore,
            IAlgoliaTaskLogger algoliaTaskLogger,
            IEventLogService eventLogService,
            IMediaFileInfoProvider mediaFileInfoProvider,
            IMediaFileUrlRetriever mediaFileUrlRetriever,
            ISearchClient searchClient)
        {
            this.eventLogService = eventLogService;
            this.algoliaIndexService = algoliaIndexService;
            this.algoliaIndexStore = algoliaIndexStore;
            this.algoliaTaskLogger = algoliaTaskLogger;
            this.mediaFileInfoProvider = mediaFileInfoProvider;
            this.mediaFileUrlRetriever = mediaFileUrlRetriever;
            this.searchClient = searchClient;
        }


        public int DeleteRecords(IEnumerable<string> objectIds, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var deletedCount = 0;
            if (objectIds == null || !objectIds.Any())
            {
                return 0;
            }

            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var responses = searchIndex.DeleteObjects(objectIds).Responses;
            foreach (var response in responses)
            {
                deletedCount += response.ObjectIDs.Count();
            }

            return deletedCount;
        }


        public List<IndicesResponse> GetStatistics()
        {
            if (searchClient == null)
            {
                return Enumerable.Empty<IndicesResponse>().ToList();
            }

            return searchClient.ListIndices().Items;
        }


        public int ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> items)
        {
            var successfulOperations = 0;

            // Group queue items based on index name
            var groups = items.GroupBy(item => item.IndexName);
            foreach (var group in groups)
            {
                try
                {
                    var algoliaIndex = algoliaIndexStore.GetIndex(group.Key);
                    if (algoliaIndex == null)
                    {
                        eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), $"Attempted to process tasks for index '{group.Key},' but the index is not registered.");
                        continue;
                    }

                    var deleteTasks = group.Where(queueItem => queueItem.Delete);
                    var updateTasks = group.Where(queueItem => !queueItem.Delete);
                    var upsertData = updateTasks.Select(queueItem => GetTreeNodeData(queueItem.Node, algoliaIndex.Type));
                    var deleteData = deleteTasks.Select(queueItem => queueItem.Node.DocumentID.ToString());

                    successfulOperations += UpsertRecords(upsertData, group.Key);
                    successfulOperations += DeleteRecords(deleteData, group.Key);
                }
                catch (InvalidOperationException ex)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), ex.Message);
                }
                catch (ArgumentNullException ex)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaClient), nameof(ProcessAlgoliaTasks), ex.Message);
                }
            }

            return successfulOperations;
        }


        public void Rebuild(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var algoliaIndex = algoliaIndexStore.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"The index '{indexName}' is not registered.");
            }

            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            searchIndex.ClearObjects();

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

            algoliaTaskLogger.LogTasks(indexedNodes.Select(node =>
                new AlgoliaQueueItem
                {
                    IndexName = algoliaIndex.IndexName,
                    Node = node,
                    Delete = false
                }
            ));
        }


        public int UpsertRecords(IEnumerable<JObject> dataObjects, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var upsertedCount = 0;
            if (dataObjects == null || !dataObjects.Any())
            {
                return 0;
            }

            var searchIndex = algoliaIndexService.InitializeIndex(indexName);
            var responses = searchIndex.SaveObjects(dataObjects).Responses;
            foreach (var response in responses)
            {
                upsertedCount += response.ObjectIDs.Count();
            }

            return upsertedCount;
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
            var strValue = ValidationHelper.GetString(nodeValue, String.Empty);
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


        private JObject GetTreeNodeData(TreeNode node, Type searchModelType)
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
            MapTreeNodeProperties(node, data, searchModelType);
            MapCommonProperties(node, data);

            return data;
        }


        /// <summary>
        /// Locates the registered search model properties which match the property names of the passed
        /// <paramref name="node"/> and sets the <paramref name="data"/> values from the <paramref name="node"/>.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> to load values from.</param>
        /// <param name="data">The dynamic data that will be passed to Algolia.</param>
        /// <param name="searchModelType">The class of the Algolia search model.</param>
        private void MapTreeNodeProperties(TreeNode node, JObject data, Type searchModelType)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new DecimalPrecisionConverter());

            var searchModel = Activator.CreateInstance(searchModelType);
            PropertyInfo[] properties = searchModel.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.DeclaringType == typeof(AlgoliaSearchModel))
                {
                    continue;
                }

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
    }
}
