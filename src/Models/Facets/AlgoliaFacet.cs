namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Represents an Algolia faceted attribute's value.
    /// </summary>
    public sealed class AlgoliaFacet
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
        /// The value of the facet.
        /// </summary>
        public string Value
        {
            get;
            set;
        }


        /// <summary>
        /// The display name of the facet's value.
        /// </summary>
        public string DisplayValue
        {
            get;
            set;
        }


        /// <summary>
        /// The number of hits that will be returned when this facet
        /// is used within an Algolia search.
        /// </summary>
        public long Count
        {
            get;
            set;
        }


        /// <summary>
        /// True if the facet was used in a previous Algolia search.
        /// </summary>
        public bool IsChecked
        {
            get;
            set;
        }
    }
}
