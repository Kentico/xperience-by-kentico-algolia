using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Admin;

public partial interface IAlgoliaIndexLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
