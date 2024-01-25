using CMS.Base;
using CMS.Localization;

using Kentico.Xperience.Algolia.Resources;

[assembly: RegisterLocalizationResource(typeof(AlgoliaResources), SystemContext.SYSTEM_CULTURE_NAME)]
namespace Kentico.Xperience.Algolia.Resources;

internal class AlgoliaResources
{
    public AlgoliaResources()
    { 
    }
}
