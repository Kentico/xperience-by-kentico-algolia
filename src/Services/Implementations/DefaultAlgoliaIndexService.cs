using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;

using CMS.Helpers;

using Kentico.Xperience.Algolia.Attributes;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexService"/>.
    /// </summary>
    internal class DefaultAlgoliaIndexService : IAlgoliaIndexService
    {
        private readonly ISearchClient searchClient;
        private readonly Dictionary<string, IndexSettings> cachedSettings = new();


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaIndexService"/> class.
        /// </summary>
        public DefaultAlgoliaIndexService(ISearchClient searchClient)
        {
            this.searchClient = searchClient;
        }


        /// <inheritdoc />
        public ISearchIndex InitializeIndex(string indexName)
        {
            var algoliaIndex = IndexStore.Instance.Get(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");
            }

            if (!cachedSettings.TryGetValue(indexName, out IndexSettings indexSettings))
            {
                indexSettings = GetIndexSettings(algoliaIndex.Type);
                cachedSettings.Add(indexName, indexSettings);
            }

            var searchIndex = searchClient.InitIndex(indexName);
            searchIndex.SetSettings(indexSettings);

            return searchIndex;
        }


        private string GetFilterablePropertyName(PropertyInfo property)
        {
            var attr = property.GetCustomAttributes<FacetableAttribute>(false).FirstOrDefault();
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


        /// <summary>
        /// Gets the <see cref="IndexSettings"/> of the Algolia index.
        /// </summary>
        /// <param name="searchModel">The index search model class.</param>
        /// <returns>The index settings.</returns>
        /// <exception cref="ArgumentNullException" />
        private IndexSettings GetIndexSettings(Type searchModel)
        {
            if (searchModel == null)
            {
                throw new ArgumentNullException(nameof(searchModel));
            }

            var searchableProperties = searchModel.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, typeof(SearchableAttribute)));
            var retrievableProperties = searchModel.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, typeof(RetrievableAttribute)));
            var facetableProperties = searchModel.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => Attribute.IsDefined(prop, typeof(FacetableAttribute)));

            return new IndexSettings()
            {
                SearchableAttributes = OrderSearchableProperties(searchableProperties),
                AttributesToRetrieve = retrievableProperties.Select(p => p.Name).ToList(),
                AttributesForFaceting = facetableProperties.Select(GetFilterablePropertyName).ToList()
            };
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