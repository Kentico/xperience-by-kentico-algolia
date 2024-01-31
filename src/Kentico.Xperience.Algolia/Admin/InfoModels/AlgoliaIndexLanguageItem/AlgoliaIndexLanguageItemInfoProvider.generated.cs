using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Class providing <see cref="AlgoliaIndexLanguageItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaIndexLanguageItemInfoProvider))]
public partial class AlgoliaIndexLanguageInfoProvider : AbstractInfoProvider<AlgoliaIndexLanguageItemInfo, AlgoliaIndexLanguageInfoProvider>, IAlgoliaIndexLanguageItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaIndexLanguageInfoProvider"/> class.
    /// </summary>
    public AlgoliaIndexLanguageInfoProvider()
        : base(AlgoliaIndexLanguageItemInfo.TYPEINFO)
    {
    }
}
