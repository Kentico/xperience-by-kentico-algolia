using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia;

/// <summary>
/// Initializes page event handlers, and ensures the thread queue workers for processing Algolia tasks.
/// </summary>
internal class AlgoliaSearchModule : Module
{
    private IAlgoliaTaskLogger algoliaTaskLogger;
    private IAppSettingsService appSettingsService;
    private IConversionService conversionService;
    private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "AlgoliaSearchDisableIndexing";


    private bool IndexingDisabled
    {
        get
        {
            return conversionService.GetBoolean(appSettingsService[APP_SETTINGS_KEY_INDEXING_DISABLED], false);
        }
    }


    /// <inheritdoc/>
    public AlgoliaSearchModule() : base(nameof(AlgoliaSearchModule))
    {
    }


    /// <inheritdoc/>
    protected override void OnInit()
    {
        base.OnInit();

        Service.Resolve<AlgoliaModuleInstaller>().Install();
        algoliaTaskLogger = Service.Resolve<IAlgoliaTaskLogger>();
        appSettingsService = Service.Resolve<IAppSettingsService>();
        conversionService = Service.Resolve<IConversionService>();


        AddRegisteredIndices().Wait();
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
        if (IndexingDisabled)
        {
            return;
        }
        var publishedEvent = (WebPageEventArgsBase)e;
        var indexedItemModel = new IndexedItemModel
        {
            LanguageCode = publishedEvent.ContentLanguageName,
            ClassName = publishedEvent.ContentTypeName,
            ChannelName = publishedEvent.WebsiteChannelName,
            WebPageItemGuid = publishedEvent.Guid,
            WebPageItemTreePath = publishedEvent.TreePath,
        };

        var task = algoliaTaskLogger?.HandleEvent(indexedItemModel, e.CurrentHandler.Name);
        task.Wait();
    }

    private void HandleContentItemEvent(object? sender, CMSEventArgs e)
    {
        if (IndexingDisabled)
        {
            return;
        }
        var publishedEvent = (ContentItemEventArgsBase)e;

        var indexedContentItemModel = new IndexedContentItemModel
        {
            LanguageCode = publishedEvent.ContentLanguageName,
            ClassName = publishedEvent.ContentTypeName,
            ContentItemGuid = publishedEvent.Guid
        };

        var task = algoliaTaskLogger?.HandleContentItemEvent(indexedContentItemModel, e.CurrentHandler.Name);

        task.Wait();
    }

    public static async Task AddRegisteredIndices()
    {
        var configurationStorageService = Service.Resolve<IConfigurationStorageService>();
        var indices = await configurationStorageService.GetAllIndexData();

        IndexStore.Instance.AddIndices(indices);
    }
}