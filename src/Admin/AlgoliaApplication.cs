using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Algolia.Admin;

[assembly: UIApplication(typeof(AlgoliaApplication), "Algolia",
                         "Algolia", BaseApplicationCategories.DEVELOPMENT,
                          Icons.Magnifier, TemplateNames.SECTION_LAYOUT)]
namespace Kentico.Xperience.Algolia.Admin
{
    internal class AlgoliaApplication : ApplicationPage
    {
    }
}
