using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Class providing <see cref="AlgoliaIncludedPathItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaIncludedPathItemInfoProvider))]
public partial class AlgoliaIncludedPathItemInfoProvider : AbstractInfoProvider<AlgoliaIncludedPathItemInfo, AlgoliaIncludedPathItemInfoProvider>, IAlgoliaIncludedPathItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaIncludedPathItemInfoProvider"/> class.
    /// </summary>
    public AlgoliaIncludedPathItemInfoProvider()
        : base(AlgoliaIncludedPathItemInfo.TYPEINFO)
    {
    }
}
