using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaObjectGenerator"/>.
    /// </summary>
    internal class DefaultAlgoliaObjectGenerator : IAlgoliaObjectGenerator
    {
        private readonly IConversionService conversionService;
        private readonly IEventLogService eventLogService;
        private readonly IMediaFileInfoProvider mediaFileInfoProvider;
        private readonly IMediaFileUrlRetriever mediaFileUrlRetriever;
        private readonly Dictionary<string, string[]> cachedIndexedColumns = new();
        private readonly string[] ignoredPropertiesForTrackingChanges = new string[] {
            nameof(AlgoliaSearchModel.ObjectID),
            nameof(AlgoliaSearchModel.Url),
            nameof(AlgoliaSearchModel.ClassName)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaObjectGenerator"/> class.
        /// </summary>
        public DefaultAlgoliaObjectGenerator(IConversionService conversionService,
            IEventLogService eventLogService,
            IMediaFileInfoProvider mediaFileInfoProvider,
            IMediaFileUrlRetriever mediaFileUrlRetriever)
        {
            this.conversionService = conversionService;
            this.eventLogService = eventLogService;
            this.mediaFileInfoProvider = mediaFileInfoProvider;
            this.mediaFileUrlRetriever = mediaFileUrlRetriever;
        }


        /// <inheritdoc/>
        public JObject GetTreeNodeData(AlgoliaQueueItem queueItem)
        {
            var data = new JObject();
            MapChangedProperties(queueItem, data);
            MapCommonProperties(queueItem.Node, data);

            return data;
        }


        /// <inheritdoc/>
        public IEnumerable<JObject> SplitData(JObject originalData, AlgoliaIndex algoliaIndex)
        {
            // No data splitting by default
            return new JObject[] { originalData };
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
                eventLogService.LogError(nameof(DefaultAlgoliaObjectGenerator), nameof(GetAssetUrlsForColumn), $"Unable to load field definition for content type '{node.ClassName}' column name '{columnName}.'");
                return Enumerable.Empty<string>();
            }

            if (!field.DataType.Equals(FieldDataType.Assets, StringComparison.OrdinalIgnoreCase))
            {
                return Enumerable.Empty<string>();
            }

            var dataType = DataTypeManager.GetDataType(typeof(IEnumerable<AssetRelatedItem>));
            if (dataType.Convert(strValue, null, null) is not IEnumerable<AssetRelatedItem> assets)
            {
                return Enumerable.Empty<string>();
            }

            var mediaFiles = mediaFileInfoProvider.Get().ForAssets(assets);

            return mediaFiles.Select(file => mediaFileUrlRetriever.Retrieve(file).RelativePath);
        }


        /// <summary>
        /// Gets the names of all database columns which are indexed by the passed index,
        /// minus those listed in <see cref="ignoredPropertiesForTrackingChanges"/>.
        /// </summary>
        /// <param name="indexName">The index to load columns for.</param>
        /// <returns>The database columns that are indexed.</returns>
        private string[] GetIndexedColumnNames(string indexName)
        {
            if (cachedIndexedColumns.TryGetValue(indexName, out string[] value))
            {
                return value;
            }

            // Don't include properties with SourceAttribute at first, check the sources and add to list after
            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            var indexedColumnNames = algoliaIndex.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !Attribute.IsDefined(prop, typeof(SourceAttribute))).Select(prop => prop.Name).ToList();
            var propertiesWithSourceAttribute = algoliaIndex.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
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

            var indexedColumns = indexedColumnNames.ToArray();
            cachedIndexedColumns.Add(indexName, indexedColumns);

            return indexedColumns;
        }


        /// <summary>
        /// Gets the <paramref name="node"/> value using the <paramref name="property"/>
        /// name, or the property's <see cref="SourceAttribute"/> if specified.
        /// </summary>
        /// <param name="node">The <see cref="TreeNode"/> to load a value from.</param>
        /// <param name="property">The Algolia search model property.</param>
        /// <param name="searchModelType">The Algolia search model.</param>
        /// <param name="columnsToUpdate">A list of columns to retrieve values for. Columns not present
        /// in this list will return <c>null</c>.</param>
        private object GetNodeValue(TreeNode node, PropertyInfo property, Type searchModelType, IEnumerable<string> columnsToUpdate)
        {
            object nodeValue = null;
            var usedColumn = property.Name;
            if (Attribute.IsDefined(property, typeof(SourceAttribute)))
            {
                // Property uses SourceAttribute, loop through column names until a non-null value is found
                var sourceAttribute = property.GetCustomAttributes<SourceAttribute>(false).FirstOrDefault();
                foreach (var source in sourceAttribute.Sources.Where(s => columnsToUpdate.Contains(s)))
                {
                    nodeValue = node.GetValue(source);
                    if (nodeValue != null)
                    {
                        usedColumn = source;
                        break;
                    }
                }
            }
            else
            {
                if (!columnsToUpdate.Contains(property.Name))
                {
                    return null;
                }

                nodeValue = node.GetValue(property.Name);
            }

            // Convert node value to URLs if necessary
            if (nodeValue != null && Attribute.IsDefined(property, typeof(MediaUrlsAttribute)))
            {
                nodeValue = GetAssetUrlsForColumn(node, nodeValue, usedColumn);
            }

            var searchModel = Activator.CreateInstance(searchModelType) as AlgoliaSearchModel;
            nodeValue = searchModel.OnIndexingProperty(node, property.Name, usedColumn, nodeValue);

            return nodeValue;
        }


        /// <summary>
        /// Adds values to the <paramref name="data"/> by retriving the indexed columns of the index
        /// and getting values from the <see cref="AlgoliaQueueItem.Node"/>. When the <see cref="AlgoliaQueueItem.TaskType"/>
        /// is <see cref="AlgoliaTaskType.UPDATE"/>, only the <see cref="AlgoliaQueueItem.ChangedColumns"/>
        /// will be added to the <paramref name="data"/>.
        /// </summary>
        private void MapChangedProperties(AlgoliaQueueItem queueItem, JObject data)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new DecimalPrecisionConverter());

            var columnsToUpdate = new List<string>();
            var indexedColumns = GetIndexedColumnNames(queueItem.IndexName);
            if (queueItem.TaskType == AlgoliaTaskType.CREATE)
            {
                columnsToUpdate.AddRange(indexedColumns);
            }
            else if (queueItem.TaskType == AlgoliaTaskType.UPDATE)
            {
                columnsToUpdate.AddRange(queueItem.ChangedColumns.Intersect(indexedColumns));
            }

            var algoliaIndex = IndexStore.Instance.GetIndex(queueItem.IndexName);
            var properties = algoliaIndex.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                object nodeValue = GetNodeValue(queueItem.Node, prop, algoliaIndex.Type, columnsToUpdate);
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
        private static void MapCommonProperties(TreeNode node, JObject data)
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
