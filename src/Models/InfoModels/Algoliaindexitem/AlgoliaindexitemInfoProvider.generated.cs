using CMS.DataEngine;

namespace CMS
{
    /// <summary>
    /// Class providing <see cref="AlgoliaindexitemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAlgoliaindexitemInfoProvider))]
    public partial class AlgoliaindexitemInfoProvider : AbstractInfoProvider<AlgoliaindexitemInfo, AlgoliaindexitemInfoProvider>, IAlgoliaindexitemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaindexitemInfoProvider"/> class.
        /// </summary>
        public AlgoliaindexitemInfoProvider()
            : base(AlgoliaindexitemInfo.TYPEINFO)
        {
        }
    }
}