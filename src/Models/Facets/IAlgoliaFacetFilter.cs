using Algolia.Search.Models.Search;

using Microsoft.Extensions.Localization;

using System;

namespace Kentico.Xperience.AlgoliaSearch.Models.Facets
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
        /// Sets the <see cref="AlgoliaFacetedAttribute.DisplayName"/> of each facet within
        /// <see cref="FacetedAttributes"/>. The key searched within the given <see cref="IStringLocalizer"/>
        /// is in the format <i>algolia.facet.[AttributeName]</i>. Also sets the <see cref="AlgoliaFacet.DisplayValue"/>
        /// of each facet within the attribute, using a key in the format
        /// <i>algolia.facet.[AttributeName].[FacetValue]</i>.
        /// </summary>
        /// <param name="localizer">The localizer containing facet display names. See
        /// <see href="https://docs.xperience.io/multilingual-websites/setting-up-a-multilingual-user-interface/localizing-builder-components"/>.</param>
        void Localize(IStringLocalizer localizer);
    }
}
