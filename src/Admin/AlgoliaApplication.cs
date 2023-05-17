using CMS.Membership;
using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// The root application page for the Algolia integration.
    /// </summary>
    [UIPermission(SystemPermissions.VIEW)]
    internal class AlgoliaApplication : ApplicationPage
    {
        public const string IDENTIFIER = "Kentico.Xperience.Integrations.Algolia";
    }
}
