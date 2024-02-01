using CMS.Core;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;

[assembly: UIPage(
   parentType: typeof(AlgoliaApplicationPage),
   slug: "indexes",
   uiPageType: typeof(IndexListingPage),
   name: "List of registered Algolia indices",
   templateName: TemplateNames.LISTING,
   order: UIPageOrder.First)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// An admin UI page that displays statistics about the registered Algolia indexes.
/// </summary>
[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class IndexListingPage : ListingPage
{
    private readonly IAlgoliaClient algoliaClient;
    private readonly IPageUrlGenerator pageUrlGenerator;
    private readonly IAlgoliaConfigurationStorageService configurationStorageService;
    private readonly IConversionService conversionService;

    protected override string ObjectType => AlgoliaIndexItemInfo.OBJECT_TYPE;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexListingPage"/> class.
    /// </summary>
    public IndexListingPage(
        IAlgoliaClient algoliaClient,
        IPageUrlGenerator pageUrlGenerator,
        IAlgoliaConfigurationStorageService configurationStorageService,
        IConversionService conversionService)
    {
        this.algoliaClient = algoliaClient;
        this.pageUrlGenerator = pageUrlGenerator;
        this.configurationStorageService = configurationStorageService;
        this.conversionService = conversionService;
    }


    /// <inheritdoc/>
    public override async Task ConfigurePage()
    {
        if (!AlgoliaIndexStore.Instance.GetAllIndices().Any())
        {
            PageConfiguration.Callouts = new List<CalloutConfiguration>
            {
                new()
                {
                    Headline = "No indexes",
                    Content = "No Algolia indexes registered. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-algolia'>our instructions</a> to read more about creating and registering Algolia indexes.",
                    ContentAsHtml = true,
                    Type = CalloutType.FriendlyWarning,
                    Placement = CalloutPlacement.OnDesk
                }
            };
        }

        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId), "ID", defaultSortDirection: SortTypeEnum.Asc, sortable: true)
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), "Name", sortable: true, searchable: true)
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemChannelName), "Channel", searchable: true, sortable: true)
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemStrategyName), "Index Strategy", searchable: true, sortable: true)
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId), "Entries", sortable: true)
            .AddColumn(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId), "Last Updated", sortable: true);

        PageConfiguration.AddEditRowAction<IndexEditPage>();
        PageConfiguration.TableActions.AddCommand("Rebuild", nameof(Rebuild), icon: Icons.RotateRight);
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete");
        PageConfiguration.HeaderActions.AddLink<IndexCreatePage>("Create");

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public Task<ICommandResponse> Delete(int id, CancellationToken _)
    {
        bool res = configurationStorageService.TryDeleteIndex(id);
        if (res)
        {
            AlgoliaIndexStore.SetIndicies(configurationStorageService);
        }
        var response = NavigateTo(pageUrlGenerator.GenerateUrl<IndexListingPage>());

        return Task.FromResult<ICommandResponse>(response);
    }

    /// <summary>
    /// A page command which rebuilds an Algolia index.
    /// </summary>
    /// <param name="id">The ID of the row whose action was performed, which corresponds with the internal
    /// <see cref="AlgoliaIndex.Identifier"/> to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token for the action.</param>
    [PageCommand]
    public async Task<ICommandResponse<RowActionResult>> Rebuild(int id, CancellationToken cancellationToken)
    {
        var result = new RowActionResult(false);
        var index = AlgoliaIndexStore.Instance.GetIndex(id);
        if (index == null)
        {
            return ResponseFrom(result)
            .AddErrorMessage(string.Format("Error loading Algolia index with identifier {0}.", id));
        }

        try
        {
            await algoliaClient.Rebuild(index.IndexName, cancellationToken);
            return ResponseFrom(result)
                 .AddSuccessMessage("Indexing in progress. Visit your Algolia dashboard for details about the indexing process.");
        }
        catch(Exception ex)
        {
            EventLogService.LogException(nameof(IndexListingPage), nameof(Rebuild), ex);
            return ResponseFrom(result)
               .AddErrorMessage(string.Format("Errors occurred while rebuilding the '{0}' index. Please check the Event Log for more details.", index.IndexName));
        }
    }

    private AlgoliaIndexStatisticsViewModel? GetStatistic(Row row, ICollection<AlgoliaIndexStatisticsViewModel> statistics)
    {
        int indexID = conversionService.GetInteger(row.Identifier, 0);
        string indexName = AlgoliaIndexStore.Instance.GetIndex(indexID) is AlgoliaIndex index
            ? index.IndexName
            : "";

        return statistics.FirstOrDefault(s => string.Equals(s.Name, indexName, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
    {
       var result = await base.LoadData(settings, cancellationToken);

        var statistics = await algoliaClient.GetStatistics(default);
        // Add statistics for indexes that are registered but not created in Algolia
        AddMissingStatistics(ref statistics);

        if (PageConfiguration.ColumnConfigurations is not List<ColumnConfiguration> columns)
        {
            return result;
        }

        int entriesColIndex = columns.FindIndex(c => c.Caption == "Entries");
        int updatedColIndex = columns.FindIndex(c => c.Caption == "Last Updated");

        foreach (var row in result.Rows)
        {
            if (row.Cells is not List<Cell> cells)
            {
                continue;
            }

            var stats = GetStatistic(row, statistics);

            if (stats is null)
            {
                continue;
            }

            if (cells[entriesColIndex] is StringCell entriesCell)
            {
                entriesCell.Value = stats.Entries.ToString();
            }
            if (cells[updatedColIndex] is StringCell updatedCell)
            {
                updatedCell.Value = stats.UpdatedAt.ToLocalTime().ToString();
            }
        }

        return result;
    }


    private static void AddMissingStatistics(ref ICollection<AlgoliaIndexStatisticsViewModel> statistics)
    {
        foreach (string indexName in AlgoliaIndexStore.Instance.GetAllIndices().Select(i => i.IndexName))
        {
            if (!statistics.Any(stat => stat.Name?.Equals(indexName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                statistics.Add(new AlgoliaIndexStatisticsViewModel
                {
                    Name = indexName,
                    Entries = 0,
                    UpdatedAt = DateTime.MinValue
                });
            }
        }
    }
}
