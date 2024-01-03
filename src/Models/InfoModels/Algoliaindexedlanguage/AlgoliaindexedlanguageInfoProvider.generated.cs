using CMS.DataEngine;

namespace CMS
{
    /// <summary>
    /// Class providing <see cref="AlgoliaindexedlanguageInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAlgoliaindexedlanguageInfoProvider))]
    public partial class AlgoliaindexedlanguageInfoProvider : AbstractInfoProvider<AlgoliaindexedlanguageInfo, AlgoliaindexedlanguageInfoProvider>, IAlgoliaindexedlanguageInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaindexedlanguageInfoProvider"/> class.
        /// </summary>
        public AlgoliaindexedlanguageInfoProvider()
            : base(AlgoliaindexedlanguageInfo.TYPEINFO)
        {
        }
    }
}