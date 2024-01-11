using CMS;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;

[assembly: RegisterModule(typeof(AlgoliaAdminModule))]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// An administration module which registers client scripts for Algolia.
/// </summary>
internal class AlgoliaAdminModule : AdminModule
{
    /// <inheritdoc/>
    public AlgoliaAdminModule()
        : base(nameof(AlgoliaAdminModule))
    {
    }


    /// <inheritdoc/>
    protected override void OnInit()
    {
        base.OnInit();

        RegisterClientModule("kentico", "xperience-integrations-algolia");
    }
}
