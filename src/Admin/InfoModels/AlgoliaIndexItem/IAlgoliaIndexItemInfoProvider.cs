using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

public partial interface IAlgoliaIndexItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}