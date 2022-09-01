using System;

using Algolia.Search.Models.Search;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Defines methods for creating Algolia faceting interfaces and filtering queries based
    /// on the selected facets.
    /// </summary>
    public interface IAlgoliaFacetFilter
    {
        /// <summary>
        /// A collection of an Algolia index's faceted attributes and the available facets.
        /// </summary>
        AlgoliaFacetedAttribute[] FacetedAttributes
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the facet filters to be added to the <see cref="Query.Filters"/> to
        /// filter an Algolia search based on selected facets and their values.
        /// </summary>
        /// <param name="searchModelType">The Algolia search model that is being used in
        /// the current query. If null, all facet filters will use the "OR" condition.</param>
        string GetFilter(Type searchModelType = null);


        /// <summary>
        /// Updates the <see cref="FacetedAttributes"/> with the facets and counts returned from
        /// an Algolia search. The checked state of facets used in the search are persisted.
        /// </summary>
        /// <param name="config">The configuration to use when updating the facets.</param>
        public void UpdateFacets(FacetConfiguration config);
    }
}
