using CMS.DataEngine;

namespace CMS
{
    /// <summary>
    /// Class providing <see cref="AlgoliaincludedpathitemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAlgoliaincludedpathitemInfoProvider))]
    public partial class AlgoliaincludedpathitemInfoProvider : AbstractInfoProvider<AlgoliaincludedpathitemInfo, AlgoliaincludedpathitemInfoProvider>, IAlgoliaincludedpathitemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaincludedpathitemInfoProvider"/> class.
        /// </summary>
        public AlgoliaincludedpathitemInfoProvider()
            : base(AlgoliaincludedpathitemInfo.TYPEINFO)
        {
        }
    }
}