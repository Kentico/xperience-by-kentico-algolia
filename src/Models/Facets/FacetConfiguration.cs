using System;
using System.Collections.Generic;

using Algolia.Search.Models.Search;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Contains the facets and settings to use in <see cref="IAlgoliaFacetFilter.UpdateFacets"/>
    /// when performing a faceted search.
    /// </summary>
    public sealed class FacetConfiguration
    {
        /// <summary>
        /// The <see cref="SearchResponse{T}.Facets"/> from an Algolia search.
        /// </summary>
        public Dictionary<string, Dictionary<string, long>> ResponseFacets
        {
            get;
            private set;
        }


        /// <summary>
        /// A collection of faceted attributes or their values, in the formats
        /// <i>[AttributeName]</i> and <i>[AttributeName].[FacetValue]</i> respectively.
        /// When updating facets, the <see cref="AlgoliaFacetedAttribute.DisplayName"/> and
        /// <see cref="AlgoliaFacet.DisplayValue"/> are updated with the values of this property.
        /// </summary>
        public Dictionary<string, string> DisplayNames
        {
            get;
            private set;
        }


        /// <summary>
        /// If <c>true</c>, facets that have a count of zero are not removed when
        /// updating facets.
        /// </summary>
        public bool DisplayEmptyFacets
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FacetConfiguration"/> class.
        /// </summary>
        /// <param name="responseFacets">The <see cref="SearchResponse{T}.Facets"/> from an Algolia search.</param>
        /// <param name="displayNames">A collection of faceted attributes or their values, in the formats
        /// <i>[AttributeName]</i> and <i>[AttributeName].[FacetValue]</i> respectively.</param>
        /// <param name="displayEmptyFacets">If <c>true</c>, facets that have a count of zero are not removed
        /// when updating facets.</param>
        /// <exception cref="ArgumentNullException" />
        public FacetConfiguration(Dictionary<string, Dictionary<string, long>> responseFacets, Dictionary<string, string> displayNames = null, bool displayEmptyFacets = false)
        {
            ResponseFacets = responseFacets ?? throw new ArgumentNullException(nameof(responseFacets));
            DisplayNames = displayNames ?? new Dictionary<string, string>();
            DisplayEmptyFacets = displayEmptyFacets;
        }
    }
}
