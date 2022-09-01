using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Kentico.Xperience.Algolia.Attributes;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Contains the faceted attributes of an Algolia index and the filter state for
    /// a faceted search interface.
    /// </summary>
    public sealed class AlgoliaFacetFilter : IAlgoliaFacetFilter
    {
        /// <inheritdoc/>
        public AlgoliaFacetedAttribute[] FacetedAttributes
        {
            get;
            set;
        } = Array.Empty<AlgoliaFacetedAttribute>();


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaFacetFilter"/> class.
        /// </summary>
        public AlgoliaFacetFilter()
        {
        }


        /// <inheritdoc/>
        public void UpdateFacets(FacetConfiguration config)
        {
            // Get previous facets that are checked to persist checked state
            var checkedFacetValues = new List<string>();
            foreach (var facetedAttribute in FacetedAttributes)
            {
                checkedFacetValues.AddRange(facetedAttribute.Facets.Where(facet => facet.IsChecked).Select(facet => facet.Value));
            }
            
            var facets = config.ResponseFacets.Select(dict =>
                new AlgoliaFacetedAttribute
                {
                    Attribute = dict.Key,
                    DisplayName = dict.Key,
                    Facets = dict.Value.Select(facet =>
                        new AlgoliaFacet
                        {
                            Attribute = dict.Key,
                            Value = facet.Key,
                            Count = facet.Value,
                            IsChecked = checkedFacetValues.Contains(facet.Key)
                        }
                    ).ToArray()
                }
            ).ToList();

            if (config.DisplayEmptyFacets && FacetedAttributes.Any())
            {
                var allFacets = AddEmptyFacets(facets);
                FacetedAttributes = Localize(allFacets, config);
                return;
            }

            FacetedAttributes = Localize(facets, config);
        }


        /// <inheritdoc/>
        public string GetFilter(Type searchModelType = null)
        {
            var checkedFacets = new List<AlgoliaFacet>();
            foreach (var facetedAttribute in FacetedAttributes)
            {
                checkedFacets.AddRange(facetedAttribute.Facets.Where(facet => facet.IsChecked));
            }

            var groupedFacets = checkedFacets.GroupBy(facet => facet.Attribute);
            var attributeFilters = groupedFacets.Select(group => {

                var joinString = " OR ";
                if (searchModelType != null)
                {
                    var facetedProperty = searchModelType.GetProperty(group.Key);
                    var facetableAttribute = facetedProperty.GetCustomAttribute<FacetableAttribute>(false);
                    if (facetableAttribute != null && facetableAttribute.UseAndCondition)
                    {
                        joinString = " AND ";
                    }
                }

                var facets = String.Join(joinString, group.Select(facet => $"{facet.Attribute}:{facet.Value}"));
                return $"({facets})";
            });

            return String.Join(" AND ", attributeFilters);
        }


        private List<AlgoliaFacetedAttribute> AddEmptyFacets(List<AlgoliaFacetedAttribute> facets)
        {
            // Loop through all facets present in previous search
            foreach (var previousFacetedAttribute in FacetedAttributes)
            {
                var matchingFacetFromResponse = facets.FirstOrDefault(facet => facet.Attribute == previousFacetedAttribute.Attribute);
                if (matchingFacetFromResponse == null)
                {
                    // Previous attribute was not returned by Algolia, add to new facet list
                    facets.Add(previousFacetedAttribute);
                    continue;
                }

                // Loop through each facet value in previous facet attribute
                foreach (var previousFacet in previousFacetedAttribute.Facets)
                {
                    if (matchingFacetFromResponse.Facets.Select(facet => facet.Value).Contains(previousFacet.Value))
                    {
                        // The facet value was returned by Algolia search, don't add
                        continue;
                    }

                    // The facet value wasn't returned by Algolia search, display with 0 count
                    previousFacet.Count = 0;
                    var newFacetList = matchingFacetFromResponse.Facets.ToList();
                    newFacetList.Add(previousFacet);
                    matchingFacetFromResponse.Facets = newFacetList.ToArray();
                }
            }

            // Sort the facet values. Usually handled by Algolia, but we are modifying the list
            foreach (var facetedAttribute in facets)
            {
                facetedAttribute.Facets = facetedAttribute.Facets.OrderBy(facet => facet.Value).ToArray();
            }

            return facets;
        }


        private AlgoliaFacetedAttribute[] Localize(List<AlgoliaFacetedAttribute> facets, FacetConfiguration config)
        {
            foreach (var facetedAttribute in facets)
            {
                if (config.DisplayNames.TryGetValue(facetedAttribute.Attribute, out string attributeDisplayName))
                {
                    facetedAttribute.DisplayName = attributeDisplayName;
                }
                else
                {
                    facetedAttribute.DisplayName = facetedAttribute.Attribute;
                }

                foreach (var facet in facetedAttribute.Facets)
                {
                    if (config.DisplayNames.TryGetValue($"{facet.Attribute}.{facet.Value}", out string facetDisplayValue))
                    {
                        facet.DisplayValue = facetDisplayValue;
                    }
                    else
                    {
                        facet.DisplayValue = facet.Value;
                    }
                }
            }

            return facets.ToArray();
        }
    }
}