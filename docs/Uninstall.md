# Upgrades

## Uninstall

This integration programmatically inserts custom module classes and their configuration into the Xperience solution on startup (see `AlgoliaModuleInstaller.cs`).

To remove this configuration and the added database tables perform one of the following sets of changes to your solution:

### Using Continuous Integration (CI)

1. Remove the `Kentico.Xperience.Algolia` NuGet package from the solution
1. Remove any code references to the package and recompile your solution
1. If you are using Xperience's Continuous Integration (CI), delete the files with the paths from your CI repository folder:

   - `\App_Data\CIRepository\@global\cms.class\kenticoAlgolia.*\**`
   - `\App_Data\CIRepository\@global\cms.class\kentico.xperience.Algolia\**`
   - `\App_Data\CIRepository\@global\kenticoAlgolia.*\**`

1. Run a CI restore, which will clean up the database tables and `CMS_Class` records.

### No Continuous Integration

If you are not using CI run the following SQL _after_ removing the NuGet package from the solution:

```sql
drop table KenticoAlgolia_AlgoliaContentTypeItem
drop table KenticoAlgolia_AlgoliaIncludedPathItem
drop table KenticoAlgolia_AlgoliaIndexLanguageItem
drop table KenticoAlgolia_AlgoliaIndexItem

delete
FROM [dbo].[CMS_Class] where ClassName like 'kenticoalgolia%'

delete
from [CMS_Resource] where ResourceName = 'Kentico.Xperience.Algolia'
```

> Note: there is currently no way to migrate index configuration in the database between versions of this integration in the case that the database schema includes breaking changes. This feature could be added in a future update.
