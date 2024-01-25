using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Algolia.Admin;

[assembly: UIApplication(
    identifier: AlgoliaApplicationPage.IDENTIFIER,
    type: typeof(AlgoliaApplicationPage),
    slug: "algolia",
    name: "Search",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Magnifier,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// The root application page for the Algolia integration.
/// </summary>
[UIPermission(SystemPermissions.VIEW)]
internal class AlgoliaApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "Kentico.Xperience.Integrations.Algolia";
}
