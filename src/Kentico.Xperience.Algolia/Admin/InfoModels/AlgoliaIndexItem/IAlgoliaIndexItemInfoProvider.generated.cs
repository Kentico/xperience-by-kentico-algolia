using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

/// <summary>
/// Declares members for <see cref="AlgoliaIndexItemInfo"/> management.
/// </summary>
public partial interface IAlgoliaIndexItemInfoProvider : IInfoProvider<AlgoliaIndexItemInfo>, IInfoByIdProvider<AlgoliaIndexItemInfo>, IInfoByNameProvider<AlgoliaIndexItemInfo>
{
}