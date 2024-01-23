using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

public partial interface IAlgoliaIncludedPathItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}