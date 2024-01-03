using CMS.DataEngine;

namespace CMS
{
    /// <summary>
    /// Class providing <see cref="AlgoliacontenttypeitemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAlgoliacontenttypeitemInfoProvider))]
    public partial class AlgoliacontenttypeitemInfoProvider : AbstractInfoProvider<AlgoliacontenttypeitemInfo, AlgoliacontenttypeitemInfoProvider>, IAlgoliacontenttypeitemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliacontenttypeitemInfoProvider"/> class.
        /// </summary>
        public AlgoliacontenttypeitemInfoProvider()
            : base(AlgoliacontenttypeitemInfo.TYPEINFO)
        {
        }
    }
}