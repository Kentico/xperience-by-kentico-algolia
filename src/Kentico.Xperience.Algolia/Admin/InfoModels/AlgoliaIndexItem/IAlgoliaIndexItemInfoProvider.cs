using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

public partial interface IAlgoliaIndexItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
