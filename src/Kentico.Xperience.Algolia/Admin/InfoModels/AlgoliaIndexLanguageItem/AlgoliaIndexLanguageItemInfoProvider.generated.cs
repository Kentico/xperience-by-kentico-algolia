using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// Class providing <see cref="AlgoliaIndexLanguageItemInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAlgoliaIndexLanguageItemInfoProvider))]
    public partial class AlgoliaIndexedLanguageInfoProvider : AbstractInfoProvider<AlgoliaIndexLanguageItemInfo, AlgoliaIndexedLanguageInfoProvider>, IAlgoliaIndexLanguageItemInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgoliaIndexedLanguageInfoProvider"/> class.
        /// </summary>
        public AlgoliaIndexedLanguageInfoProvider()
            : base(AlgoliaIndexLanguageItemInfo.TYPEINFO)
        {
        }
    }
}
