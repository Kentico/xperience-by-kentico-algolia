using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Models;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: RegisterImplementation(typeof(IAlgoliaRegistrationService), typeof(DefaultAlgoliaRegistrationService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaRegistrationService"/>.
    /// </summary>
    internal class DefaultAlgoliaRegistrationService : IAlgoliaRegistrationService
    {
        private readonly IAlgoliaService algoliaSearchService;
        private readonly IEventLogService eventLogService;
        private readonly ISearchClient searchClient;
        private readonly IAlgoliaIndexService algoliaIndexService;
        private readonly IAlgoliaIndexStore algoliaIndexStore;
        private readonly List<AlgoliaIndex> registeredIndexes = new List<AlgoliaIndex>();
        private readonly string[] ignoredPropertiesForTrackingChanges = new string[] {
            nameof(AlgoliaSearchModel.ObjectID),
            nameof(AlgoliaSearchModel.Url),
            nameof(AlgoliaSearchModel.ClassName)
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaRegistrationService"/> class.
        /// </summary>
        public DefaultAlgoliaRegistrationService(IAlgoliaService algoliaSearchService,
            IEventLogService eventLogService,
            ISearchClient searchClient,
            IAlgoliaIndexService algoliaIndexService,
            IAlgoliaIndexStore algoliaIndexStore)
        {
            this.algoliaSearchService = algoliaSearchService;
            this.eventLogService = eventLogService;
            this.searchClient = searchClient;
            this.algoliaIndexService = algoliaIndexService;
            this.algoliaIndexStore = algoliaIndexStore;
        }


        public IEnumerable<AlgoliaIndex> GetAllIndexes()
        {
            return registeredIndexes;
        }


        public AlgoliaIndex GetIndex(string indexName)
        {
            return registeredIndexes.FirstOrDefault(i => i.IndexName.Equals(indexName, StringComparison.OrdinalIgnoreCase));
        }


        public IndexSettings GetIndexSettings(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            var alogliaIndex = GetIndex(indexName);
            if (alogliaIndex == null)
            {
                return null;
            }

            var searchableProperties = alogliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SearchableAttribute)));
            var retrievablProperties = alogliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(RetrievableAttribute)));
            var facetableProperties = alogliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FacetableAttribute)));
            
            return new IndexSettings()
            {
                SearchableAttributes = OrderSearchableProperties(searchableProperties),
                AttributesToRetrieve = retrievablProperties.Select(p => p.Name).ToList(),
                AttributesForFaceting = facetableProperties.Select(GetFilterablePropertyName).ToList()
            };
        }


        public string[] GetIndexedColumnNames(string indexName)
        {
            var alogliaIndex = GetIndex(indexName);
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


        public bool IsNodeAlgoliaIndexed(TreeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            foreach (var index in registeredIndexes)
            {
                if (IsNodeIndexedByIndex(node, index.IndexName))
                {
                    return true;
                }
            }

            return false;
        }


        public bool IsNodeIndexedByIndex(TreeNode node, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var alogliaIndex = GetIndex(indexName);
            if (alogliaIndex == null)
            {
                eventLogService.LogError(nameof(DefaultAlgoliaRegistrationService), nameof(IsNodeIndexedByIndex), $"Error loading registered Algolia index '{indexName}.'");
                return false;
            }
            
            var includedPathAttributes = alogliaIndex.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            foreach (var includedPathAttribute in includedPathAttributes)
            {
                var path = includedPathAttribute.AliasPath;
                var matchesPageType = (includedPathAttribute.PageTypes.Length == 0 || includedPathAttribute.PageTypes.Contains(node.ClassName));
                if (path.EndsWith("/%"))
                {
                    path = path.TrimEnd('%', '/');
                    if (node.NodeAliasPath.StartsWith(path) && matchesPageType)
                    {
                        return true;
                    }
                }
                else
                {
                    if (node.NodeAliasPath == path && matchesPageType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public void RegisterAlgoliaIndexes()
        {
            var algoliaIndex = algoliaIndexStore.Pop();
            while (algoliaIndex != null)
            {
                RegisterIndex(algoliaIndex);
                algoliaIndex = algoliaIndexStore.Pop();
            }
        }


        public void RegisterIndex(AlgoliaIndex algoliaIndex)
        {
            if (String.IsNullOrEmpty(algoliaIndex.IndexName))
            {
                eventLogService.LogError(nameof(DefaultAlgoliaRegistrationService), nameof(RegisterIndex), "Cannot register Algolia index with empty or null code name.");
                return;
            }

            if (algoliaIndex.Type == null)
            {
                eventLogService.LogError(nameof(DefaultAlgoliaRegistrationService), nameof(RegisterIndex), "Cannot register Algolia index with null search model type.");
                return;
            }

            if (GetIndex(algoliaIndex.IndexName) != null)
            {
                eventLogService.LogError(nameof(DefaultAlgoliaRegistrationService), nameof(RegisterIndex), $"Attempted to register Algolia index with name '{algoliaIndex.IndexName},' but it is already registered.");
                return;
            }

            try
            {
                registeredIndexes.Add(algoliaIndex);

                var searchIndex = algoliaIndexService.InitializeIndex(algoliaIndex.IndexName);
                var indexSettings = GetIndexSettings(algoliaIndex.IndexName);
                if (indexSettings == null)
                {
                    eventLogService.LogError(nameof(DefaultAlgoliaRegistrationService), nameof(RegisterIndex), $"Unable to load search index settings for index '{algoliaIndex.IndexName}.'");
                    return;
                }

                searchIndex.SetSettings(indexSettings);
            }
            catch (Exception ex)
            {
                registeredIndexes.Remove(algoliaIndex);
                eventLogService.LogException(nameof(DefaultAlgoliaRegistrationService), nameof(RegisterIndex), ex, additionalMessage: $"Cannot register Algolia index '{algoliaIndex.IndexName}.'");
            }
        }


        private string GetFilterablePropertyName(PropertyInfo property)
        {
            var attr = property.GetCustomAttributes<FacetableAttribute>(false).FirstOrDefault();
            if (attr.FilterOnly && attr.Searchable)
            {
                throw new InvalidOperationException("Facetable attributes cannot be both searchable and filterOnly.");
            }

            if (attr.FilterOnly)
            {
                return $"filterOnly({property.Name})";
            }
            if (attr.Searchable)
            {
                return $"searchable({property.Name})";
            }

            return property.Name;
        }


        private List<string> OrderSearchableProperties(IEnumerable<PropertyInfo> searchableProperties)
        {
            var propertiesWithAttribute = new Dictionary<string, SearchableAttribute>();
            foreach (var prop in searchableProperties)
            {
                var attr = prop.GetCustomAttributes<SearchableAttribute>(false).FirstOrDefault();
                propertiesWithAttribute.Add(prop.Name, attr);
            }

            // Remove properties without order, add to end of list later
            var propertiesWithOrdering = propertiesWithAttribute.Where(prop => prop.Value.Order >= 0);
            var sortedByOrder = propertiesWithOrdering.OrderBy(prop => prop.Value.Order);
            var groupedByOrder = sortedByOrder.GroupBy(prop => prop.Value.Order);
            var searchableAttributes = groupedByOrder.Select(group =>
                group.Select(prop =>
                {
                    if (prop.Value.Unordered)
                    {
                        return $"unordered({prop.Key})";
                    }

                    return prop.Key;
                }).Join(",")
            ).ToList();

            // Add properties without order as single items
            var propertiesWithoutOrdering = propertiesWithAttribute.Where(prop => prop.Value.Order == -1);
            foreach (var prop in propertiesWithoutOrdering)
            {
                if (prop.Value.Unordered)
                {
                    searchableAttributes.Add($"unordered({prop.Key})");
                    continue;
                }

                searchableAttributes.Add(prop.Key);
            }

            return searchableAttributes;
        }
    }
}
