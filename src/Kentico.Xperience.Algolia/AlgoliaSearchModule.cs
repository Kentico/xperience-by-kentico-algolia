using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Algolia;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Microsoft.Extensions.DependencyInjection;

[assembly: RegisterModule(typeof(AlgoliaSearchModule))]

namespace Kentico.Xperience.Algolia;

/// <summary>
/// Initializes page event handlers, and ensures the thread queue workers for processing Algolia tasks.
/// </summary>
internal class AlgoliaSearchModule : Module
{
    private IAlgoliaTaskLogger? algoliaTaskLogger;
    private IAppSettingsService? appSettingsService;
    private IConversionService? conversionService;
    private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "AlgoliaSearchDisableIndexing";

    private bool IndexingDisabled => conversionService?.GetBoolean(appSettingsService?[APP_SETTINGS_KEY_INDEXING_DISABLED], false) ?? false;

    /// <inheritdoc/>
    public AlgoliaSearchModule() : base(nameof(AlgoliaSearchModule))
    {
    }

    /// <inheritdoc/>
    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit();
        var services = parameters.Services;

        services.GetRequiredService<AlgoliaModuleInstaller>().Install();
        algoliaTaskLogger = Service.Resolve<IAlgoliaTaskLogger>();
        appSettingsService = Service.Resolve<IAppSettingsService>();
        conversionService = Service.Resolve<IConversionService>();


        AddRegisteredIndices();
        WebPageEvents.Publish.Execute += HandleEvent;
        WebPageEvents.Delete.Execute += HandleEvent;
        ContentItemEvents.Publish.Execute += HandleContentItemEvent;
        ContentItemEvents.Delete.Execute += HandleContentItemEvent;

        RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => AlgoliaQueueWorker.Current.EnsureRunningThread();
    }


    /// <summary>
    /// Called when a page is published. Logs an Algolia task to be processed later.
    /// </summary>
    private void HandleEvent(object? sender, CMSEventArgs e)
    {
        if (IndexingDisabled || e is not WebPageEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedItemModel = new IndexEventWebPageItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID,
            publishedEvent.WebsiteChannelName,
            publishedEvent.TreePath,
            publishedEvent.ParentID,
            publishedEvent.Order)
        { };

        algoliaTaskLogger?.HandleEvent(indexedItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    private void HandleContentItemEvent(object? sender, CMSEventArgs e)
    {
        if (IndexingDisabled || e is not ContentItemEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedContentItemModel = new IndexEventReusableItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID
        );

        algoliaTaskLogger?.HandleReusableItemEvent(indexedContentItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    public static void AddRegisteredIndices()
    {
        var configurationStorageService = Service.Resolve<IAlgoliaConfigurationStorageService>();
        var indices = configurationStorageService.GetAllIndexData();

        AlgoliaIndexStore.Instance.SetIndicies(indices);
    }
}
