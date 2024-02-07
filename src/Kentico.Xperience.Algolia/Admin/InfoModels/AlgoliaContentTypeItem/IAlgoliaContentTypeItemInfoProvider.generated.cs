using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Declares members for <see cref="AlgoliaContentTypeItemInfo"/> management.
/// </summary>
public partial interface IAlgoliaContentTypeItemInfoProvider : IInfoProvider<AlgoliaContentTypeItemInfo>, IInfoByIdProvider<AlgoliaContentTypeItemInfo>, IInfoByNameProvider<AlgoliaContentTypeItemInfo>
{
}
