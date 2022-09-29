using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Models.Settings;

using CMS.Helpers;

using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;

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
        public async Task<ISearchIndex> InitializeIndex(string indexName, CancellationToken cancellationToken)
        {
            var algoliaIndex = IndexStore.Instance.GetIndex(indexName);
            if (algoliaIndex == null)
            {
                throw new InvalidOperationException($"Registered index with name '{indexName}' doesn't exist.");
            }

            if (!cachedSettings.TryGetValue(indexName, out IndexSettings indexSettings))
            {
                indexSettings = GetIndexSettings(algoliaIndex);
                cachedSettings.Add(indexName, indexSettings);
            }

            var searchIndex = searchClient.InitIndex(indexName);
            await searchIndex.SetSettingsAsync(indexSettings, ct: cancellationToken);

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
        /// <param name="algoliaIndex">The index to retrieve settings for.</param>
        /// <returns>The index settings.</returns>
        /// <exception cref="ArgumentNullException" />
        private IndexSettings GetIndexSettings(AlgoliaIndex algoliaIndex)
        {
            if (algoliaIndex == null)
            {
                throw new ArgumentNullException(nameof(algoliaIndex));
            }

            var searchableProperties = algoliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(SearchableAttribute)));
            var retrievableProperties = algoliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(RetrievableAttribute)));
            var facetableProperties = algoliaIndex.Type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FacetableAttribute)));
            var indexSettings = new IndexSettings()
            {
                SearchableAttributes = OrderSearchableProperties(searchableProperties),
                AttributesToRetrieve = retrievableProperties.Select(p => p.Name).ToList(),
                AttributesForFaceting = facetableProperties.Select(GetFilterablePropertyName).ToList()
            };

            if (algoliaIndex.DistinctOptions != null)
            {
                indexSettings.Distinct = algoliaIndex.DistinctOptions.DistinctLevel;
                indexSettings.AttributeForDistinct = algoliaIndex.DistinctOptions.DistinctAttribute;
            }

            return indexSettings;
        }


        private static List<string> OrderSearchableProperties(IEnumerable<PropertyInfo> searchableProperties)
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