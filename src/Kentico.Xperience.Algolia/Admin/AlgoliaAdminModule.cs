using CMS;
using CMS.Base;
using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(typeof(AlgoliaAdminModule))]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Manages administration features and integration.
/// </summary>
internal class AlgoliaAdminModule : AdminModule
{
    private IAlgoliaConfigurationStorageService storageService = null!;
    private IServiceProvider serviceProvider = null!;

    public AlgoliaAdminModule() : base(nameof(AlgoliaAdminModule)) { }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);
        RegisterClientModule("kentico", "xperience-integrations-algolia");

        serviceProvider = parameters.Services;
        storageService = serviceProvider.GetRequiredService<IAlgoliaConfigurationStorageService>();

        ApplicationEvents.PostStart.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        var installer = serviceProvider.GetRequiredService<AlgoliaModuleInstaller>();
        installer.Install();

        AlgoliaIndexStore.SetIndicies(storageService);
    }
}
