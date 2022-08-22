namespace Kentico.Xperience.AlgoliaSearch.Models
{
    /// <summary>
    /// Algolia integration options.
    /// </summary>
    public sealed class AlgoliaOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SECTION_NAME = "xperience.algolia";


        /// <summary>
        /// Algolia application ID.
        /// </summary>
        public string ApplicationId
        {
            get;
            set;
        } = "NO_APP";


        /// <summary>
        /// Algolia API key.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        } = "NO_KEY";


        /// <summary>
        /// Public API key used for searching only.
        /// </summary>
        public string SearchKey
        {
            get;
            set;
        }
    }
}