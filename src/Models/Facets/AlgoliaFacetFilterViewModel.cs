using Kentico.Xperience.AlgoliaSearch.Attributes;

using Microsoft.Extensions.Localization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kentico.Xperience.AlgoliaSearch.Models.Facets
{
    /// <summary>
    /// Contains the faceted attributes of an Algolia index and the filter state for
    /// a faceted search interface.
    /// </summary>
    public class AlgoliaFacetFilterViewModel : IAlgoliaFacetFilter
    {
        public AlgoliaFacetedAttribute[] FacetedAttributes { get; set; } = new AlgoliaFacetedAttribute[0];


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaFacetFilterViewModel"/> class
        /// with an empty set of <see cref="FacetedAttributes"/>.
        /// </summary>
        public AlgoliaFacetFilterViewModel()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaFacetFilterViewModel"/> class
        /// with the <see cref="FacetedAttributes"/> set to the provided <paramref name="facets"/>.
        /// </summary>
        /// <param name="facets">A collection of an Algolia index's faceted attributes and the
        /// available facets.</param>
        public AlgoliaFacetFilterViewModel(AlgoliaFacetedAttribute[] facets)
        {
            FacetedAttributes = facets;
        }


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


        public void Localize(IStringLocalizer localizer)
        {
            foreach (var facetedAttribute in FacetedAttributes)
            {
                facetedAttribute.DisplayName = localizer.GetString($"algolia.facet.{facetedAttribute.Attribute}");
                foreach (var facet in facetedAttribute.Facets)
                {
                    facet.DisplayValue = localizer.GetString($"algolia.facet.{facet.Attribute}.{facet.Value}");
                }
            }
        }
    }
}