using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

/// <summary>
/// Class providing <see cref="
/// 
/// "/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaIndexItemInfoProvider))]
public partial class AlgoliaIndexItemInfoProvider : AbstractInfoProvider<AlgoliaIndexItemInfo, AlgoliaIndexItemInfoProvider>, IAlgoliaIndexItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaIndexItemInfoProvider"/> class.
    /// </summary>
    public AlgoliaIndexItemInfoProvider()
        : base(AlgoliaIndexItemInfo.TYPEINFO)
    {
    }
}