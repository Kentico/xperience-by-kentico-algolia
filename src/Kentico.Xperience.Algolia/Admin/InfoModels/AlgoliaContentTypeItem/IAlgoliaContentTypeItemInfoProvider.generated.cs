using CMS.DataEngine;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Class providing <see cref="AlgoliaContentTypeItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaContentTypeItemInfoProvider))]
public partial class AlgoliaContentTypeItemInfoProvider : AbstractInfoProvider<AlgoliaContentTypeItemInfo, AlgoliaContentTypeItemInfoProvider>, IAlgoliaContentTypeItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaContentTypeItemInfoProvider"/> class.
    /// </summary>
    public AlgoliaContentTypeItemInfoProvider()
        : base(AlgoliaContentTypeItemInfo.TYPEINFO)
    {
    }
}
