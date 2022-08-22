﻿using Algolia.Search.Models.Common;
using Algolia.Search.Models.Search;

using Kentico.Xperience.AlgoliaSearch.Attributes;
using Kentico.Xperience.AlgoliaSearch.Models.Facets;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Contains methods for common Algolia tasks.
    /// </summary>
    public interface IAlgoliaService
    {
        /// <summary>
        /// Gets the indices of the Algolia application with basic statistics.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/api-reference/api-methods/list-indices/#response"/></remarks>
        List<IndicesResponse> GetStatistics();


        /// <summary>
        /// Gets a list of faceted Algolia attributes from a search response. If a <paramref name="filter"/> is
        /// provided, the <see cref="AlgoliaFacet.IsChecked"/> property is set based on the state of the filter.
        /// </summary>
        /// <param name="facetsFromResponse">The <see cref="SearchResponse{T}.Facets"/> returned from an Algolia search.</param>
        /// <param name="filter">The <see cref="IAlgoliaFacetFilter"/> used in previous Algolia searches, containing
        /// the facets that were present and their <see cref="AlgoliaFacet.IsChecked"/> states.</param>
        /// <param name="displayEmptyFacets">If true, facets that would not return results from Algolia will be added
        /// to the returned list with a count of zero.</param>
        /// <returns>A new list of <see cref="AlgoliaFacetedAttribute"/>s that are available to filter search
        /// results.</returns>
        AlgoliaFacetedAttribute[] GetFacetedAttributes(Dictionary<string, Dictionary<string, long>> facetsFromResponse, IAlgoliaFacetFilter filter = null, bool displayEmptyFacets = true);


        /// <summary>
        /// Returns true if Algolia indexing is enabled, or if the settings key doesn't exist.
        /// </summary>
        bool IsIndexingEnabled();
    }
}
