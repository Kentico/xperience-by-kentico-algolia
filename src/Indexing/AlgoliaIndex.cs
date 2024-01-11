using Algolia.Search.Models.Settings;
using Kentico.Xperience.Algolia.Admin;
using System;
using System.Collections.Generic;

namespace Kentico.Xperience.Algolia.Indexing
{
    /// <summary>
    /// Represents the configuration of an Algolia index.
    /// </summary>
    public sealed class AlgoliaIndex
    {
        /// <summary>
        /// The type of the class which extends <see cref="IAlgoliaIndexingStrategy"/>.
        /// </summary>
        public Type AlgoliaIndexingStrategyType
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
        public List<string> LanguageNames
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

        internal IEnumerable<AlgoliaIndexIncludedPath> IncludedPaths
        {
            get;
            set;
        }

        internal AlgoliaIndex(string indexName, string webSiteChannelName, List<string> languageNames, int identifier, IEnumerable<AlgoliaIndexIncludedPath> paths, Type? luceneIndexingStrategyType = null)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            IndexName = indexName;

            Identifier = identifier;
            WebSiteChannelName = webSiteChannelName;
            LanguageNames = languageNames;

            IncludedPaths = paths;

            AlgoliaIndexingStrategyType = luceneIndexingStrategyType ?? typeof(DefaultAlgoliaIndexingStrategy);
        }
    }
}
