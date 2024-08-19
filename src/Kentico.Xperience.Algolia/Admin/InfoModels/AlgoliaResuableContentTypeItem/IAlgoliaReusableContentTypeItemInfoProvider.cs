using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Declares members for <see cref="AlgoliaReusableContentTypeItemInfo"/> management.
/// </summary>
public partial interface IAlgoliaReusableContentTypeItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
