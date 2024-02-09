using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Declares members for <see cref="AlgoliaIndexLanguageItemInfo"/> management.
/// </summary>
public partial interface IAlgoliaIndexLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
