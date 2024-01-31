using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

public partial interface IAlgoliaIncludedPathItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
