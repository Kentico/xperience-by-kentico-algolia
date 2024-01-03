using Algolia.Search.Models.Settings;
using Kentico.Xperience.Algolia.Services;
using System;
using System.Collections.Generic;

namespace Kentico.Xperience.Algolia.Models
{
    public class IncludedPath
    {
        /// <summary>
        /// The node alias pattern that will be used to match pages in the content tree for indexing.
        /// </summary>
        /// <remarks>For example, "/Blogs/Products/" will index all pages under the "Products" page.</remarks>
        public string AliasPath
        {
            get;
        }


        /// <summary>
        /// A list of content types under the specified <see cref="AliasPath"/> that will be indexed.
        /// </summary>
        public string[]? ContentTypes
        {
            get;
            set;
        } = Array.Empty<string>();


        /// <summary>
        /// The internal identifier of the included path.
        /// </summary>
        internal string? Identifier
        {
            get;
            set;
        }


        /// <summary>
        /// </summary>
        /// <param name="aliasPath">The node alias pattern that will be used to match pages in the content tree
        /// for indexing.</param>
        public IncludedPath(string aliasPath) => AliasPath = aliasPath;
    }

    /// <summary>
    /// Represents the configuration of an Algolia index.
    /// </summary>
    public sealed class AlgoliaIndex
    {
        public IndexSettings IndexSettings { get; set; }

        /// <summary>
        /// The type of the class which extends <see cref="IAlgoliaIndexingStrategy"/>.
        /// </summary>
        public IAlgoliaIndexingStrategy AlgoliaIndexingStrategy
        {
            get;
        }

        /// <summary>
        /// The Name of the WebSiteChannel.
        /// </summary>
        public string WebSiteChannelName
        {
            get;
        }

        /// <summary>
        /// The Language used on the WebSite on the Channel which is indexed.
        /// </summary>
        public List<string> LanguageCodes
        {
            get;
        }

        /// <summary>
        /// The code name of the Algolia index.
        /// </summary>
        public string IndexName
        {
            get;
        }

        /// <summary>
        /// An arbitrary ID used to identify the Algolia index in the admin UI.
        /// </summary>
        public int Identifier
        {
            get;
            set;
        }

        internal IEnumerable<IncludedPath> IncludedPaths
        {
            get;
            set;
        }

        public AlgoliaIndex(string indexName, string webSiteChannelName, List<string> languageCodes, int identifier, IEnumerable<IncludedPath> paths, IAlgoliaIndexingStrategy strategy)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            IndexName = indexName;

            Identifier = identifier;
            WebSiteChannelName = webSiteChannelName;
            LanguageCodes = languageCodes;

            IncludedPaths = paths;

            AlgoliaIndexingStrategy = strategy;

            IndexSettings = strategy.GetAlgoliaIndexSettings();

            IndexSettings.AttributesToRetrieve.Add("Url");
            IndexSettings.AttributesToRetrieve.Add("objectID");
            IndexSettings.AttributesToRetrieve.Add(nameof(IndexedItemModel.ClassName));
            IndexSettings.AttributesToRetrieve.Add(nameof(IndexedItemModel.LanguageCode));
        }
    }
}
