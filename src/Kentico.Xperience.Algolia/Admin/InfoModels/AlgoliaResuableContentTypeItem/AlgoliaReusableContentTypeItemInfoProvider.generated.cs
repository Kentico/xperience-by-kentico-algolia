using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Class providing <see cref="AlgoliaReusableContentTypeItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaReusableContentTypeItemInfoProvider))]
public partial class AlgoliaReusableContentTypeItemInfoProvider : AbstractInfoProvider<AlgoliaReusableContentTypeItemInfo, AlgoliaReusableContentTypeItemInfoProvider>, IAlgoliaReusableContentTypeItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaReusableContentTypeItemInfoProvider"/> class.
    /// </summary>
    public AlgoliaReusableContentTypeItemInfoProvider()
        : base(AlgoliaReusableContentTypeItemInfo.TYPEINFO)
    {
    }
}
