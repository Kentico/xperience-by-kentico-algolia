namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Represents an Algolia faceted attribute.
    /// </summary>
    public sealed class AlgoliaFacetedAttribute
    {
        /// <summary>
        /// The name of the faceted attribute.
        /// </summary>
        public string Attribute
        {
            get;
            set;
        }


        /// <summary>
        /// The display name of the faceted attribute.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Available facets of the faceted attibute.
        /// </summary>
        public AlgoliaFacet[] Facets
        {
            get;
            set;
        }
    }
}
