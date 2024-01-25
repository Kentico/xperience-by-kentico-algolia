using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

public partial interface IAlgoliaIndexLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}