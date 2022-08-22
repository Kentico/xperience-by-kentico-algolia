using System;
using System.Collections.Generic;

namespace Kentico.Xperience.AlgoliaSearch.Models
{
    /// <summary>
    /// Represents the configuration of an Algolia index.
    /// </summary>
    public class AlgoliaIndex
    {
        /// <summary>
        /// The type of the class which extends <see cref="AlgoliaSearchModel"/>.
        /// </summary>
        public Type Type
        {
            get;
            set;
        }


        /// <summary>
        /// The code name of the Algolia index.
        /// </summary>
        public string IndexName
        {
            get;
            set;
        }


        /// <summary>
        /// The code names of the sites whose pages will be included in the index. If empty, all sites
        /// are included.
        /// </summary>
        public IEnumerable<string> SiteNames
        {
            get;
            set;
        }
    }
}
