using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;

using CMS;
using CMS.Core;
using CMS.Helpers;
using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: RegisterImplementation(typeof(IAlgoliaIndexService), typeof(DefaultAlgoliaIndexService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexService"/>.
    /// </summary>
    internal class DefaultAlgoliaIndexService : IAlgoliaIndexService
    {
        private readonly IAlgoliaIndexStore algoliaIndexStore;
        private readonly ISearchClient searchClient;
        private readonly Dictionary<string, IndexSettings> cachedSettings = new Dictionary<string, IndexSettings>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaIndexService"/> class.
        /// </summary>
        public DefaultAlgoliaIndexService(IAlgoliaIndexStore algoliaIndexStore, ISearchClient searchClient)
        {
            this.algoliaIndexStore = algoliaIndexStore;
            this.searchClient = searchClient;
        }


        public IndexSettings GetIndexSettings(Type searchModel)
        {
            if (searchModel == null)
            {
                throw new ArgumentNullException(nameof(searchModel));
            }

            var searchableProperties = searchModel.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SearchableAttribute)));
            var retrievablProperties = searchModel.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(RetrievableAttribute)));
            var facetableProperties = searchModel.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FacetableAttribute)));

            return new IndexSettings()
            {
                SearchableAttributes = OrderSearchableProperties(searchableProperties),
                AttributesToRetrieve = retrievablProperties.Select(p => p.Name).ToList(),
                AttributesForFaceting = facetableProperties.Select(GetFilterablePropertyName).ToList()
            };
        }


        public ISearchIndex InitializeIndex(string indexName)
        {
            var algoliaIndex = algoliaIndexStore.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");
            }

            IndexSettings indexSettings;
            var searchIndex = searchClient.InitIndex(indexName);
            if (!cachedSettings.TryGetValue(indexName, out indexSettings))
            {
                indexSettings = GetIndexSettings(algoliaIndex.Type);
                cachedSettings.Add(indexName, indexSettings);
            }
            
            searchIndex.SetSettings(indexSettings);

            return searchIndex;
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