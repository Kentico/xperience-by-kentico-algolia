using Algolia.Search.Models.Common;

using CMS.Core;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Kentico.Xperience.Algolia.Services;
using Action = Kentico.Xperience.Admin.Base.Action;

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
internal class IndexListingPage : ListingPageBase<ListingConfiguration>
{
    private readonly IAlgoliaClient algoliaClient;
    private readonly IPageUrlGenerator pageUrlGenerator;
    private readonly IAlgoliaConfigurationStorageService configurationStorageService;
    private readonly IUIPermissionEvaluator permissionEvaluator;
    private ListingConfiguration? mPageConfiguration;

    /// <inheritdoc/>
    public override ListingConfiguration PageConfiguration
    {
        get
        {
            mPageConfiguration ??= new ListingConfiguration()
            { 
                Caption = LocalizationService.GetString("List of indices"),
                ColumnConfigurations = new List<ColumnConfiguration>(),
                TableActions = new List<ActionConfiguration>(),
                HeaderActions = new List<ActionConfiguration>(),
                PageSizes = new List<int> { 10, 25 }
            };

            return mPageConfiguration;
            
        }
        set
        {
            mPageConfiguration = value;
        }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="IndexListingPage"/> class.
    /// </summary>
    public IndexListingPage(
        IAlgoliaClient algoliaClient,
        IPageUrlGenerator pageUrlGenerator,
        IAlgoliaConfigurationStorageService configurationStorageService,
        IUIPermissionEvaluator permissionEvaluator)
    {
        this.algoliaClient = algoliaClient;
        this.pageUrlGenerator = pageUrlGenerator;
        this.configurationStorageService = configurationStorageService;
        this.permissionEvaluator = permissionEvaluator;
    }


    /// <inheritdoc/>
    public override async Task ConfigurePage()
    {
        if (!AlgoliaIndexStore.Instance.GetAllIndices().Any())
        {
            PageConfiguration.Callouts = new List<CalloutConfiguration>
            {
                new CalloutConfiguration
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
            .AddColumn(nameof(IndicesResponse.Name), LocalizationService.GetString("Name"), defaultSortDirection: SortTypeEnum.Asc, searchable: true)
            .AddColumn(nameof(IndicesResponse.Entries), LocalizationService.GetString("Indexed items"))
            .AddColumn(nameof(IndicesResponse.LastBuildTimes), LocalizationService.GetString("Build Time"))
            .AddColumn(nameof(IndicesResponse.UpdatedAt), LocalizationService.GetString("Updated At"));

        var permissions = await GetUIPermissions();

        if (permissions.Rebuild)
        {
            PageConfiguration.TableActions.AddCommand(LocalizationService.GetString("Build index"), nameof(Rebuild), Icons.RotateRight);
        }
        if (permissions.Update)
        {
            PageConfiguration.TableActions.AddCommand("Edit", nameof(Edit));
        }
        if (permissions.Delete)
        {
            PageConfiguration.TableActions.AddCommand("Delete", nameof(Delete));
        }
        if (permissions.Create)
        {
            PageConfiguration.HeaderActions.AddLink<IndexCreatePage>("Create");
        }
        await base.ConfigurePage();
    }


    [PageCommand]
    public async Task<INavigateResponse> RowClick(int id)
       => await Task.FromResult(NavigateTo(pageUrlGenerator.GenerateUrl<IndexEditPage>(id.ToString())));

    [PageCommand]
    public async Task<INavigateResponse> Edit(int id, CancellationToken cancellationToken)
    {
        return await Task.FromResult(NavigateTo(pageUrlGenerator.GenerateUrl<IndexEditPage>(id.ToString())));
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public Task<ICommandResponse> Delete(int id, CancellationToken _)
    {
        bool res = configurationStorageService.TryDeleteIndex(id);
        if (res)
        {
            var indices = configurationStorageService.GetAllIndexData();

            AlgoliaIndexStore.Instance.SetIndicies(indices);
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


    /// <inheritdoc/>
    protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await algoliaClient.GetStatistics(cancellationToken);

            // Add statistics for indexes that are registered but not created in Algolia
            AddMissingStatistics(ref statistics);

            // Remove statistics for indexes that are not registered in this instance
            var filteredStatistics = statistics.Where(stat =>
                AlgoliaIndexStore.Instance.GetAllIndices().Any(index => index.IndexName.Equals(stat.Name, StringComparison.OrdinalIgnoreCase)));

            var searchedStatistics = DoSearch(filteredStatistics, settings.SearchTerm);
            var orderedStatistics = SortStatistics(searchedStatistics, settings);
            var rows = orderedStatistics.Select(stat => GetRow(stat));

            return new LoadDataResult
            {
                Rows = rows,
                TotalCount = rows.Count()
            };
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(IndexListingPage), nameof(LoadData), ex);
            return new LoadDataResult
            {
                Rows = Enumerable.Empty<Row>(),
                TotalCount = 0
            };
        }
    }


    private static void AddMissingStatistics(ref ICollection<IndicesResponse> statistics)
    {
        foreach (var indexName in AlgoliaIndexStore.Instance.GetAllIndices().Select(i => i.IndexName))
        {
            if (!statistics.Any(stat => stat.Name.Equals(indexName, StringComparison.OrdinalIgnoreCase)))
            {
                statistics.Add(new IndicesResponse
                {
                    Name = indexName,
                    Entries = 0,
                    LastBuildTimes = 0,
                    UpdatedAt = DateTime.MinValue
                });
            }
        }
    }


    private static IEnumerable<IndicesResponse> DoSearch(IEnumerable<IndicesResponse> statistics, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return statistics;
        }

        return statistics.Where(stat => stat.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }


    private Row GetRow(IndicesResponse statistics)
    {
        var algoliaIdex = AlgoliaIndexStore.Instance.GetIndex(statistics.Name);
        if (algoliaIdex == null)
        {
            throw new InvalidOperationException($"Unable to retrieve Algolia index with name '{statistics.Name}.'");
        }

        return new Row
        {
            Identifier = algoliaIdex.Identifier,
            Action = new Action(ActionType.Command)
            {
                Parameter = nameof(RowClick)
            },
            Cells = new List<Cell>
            {
                new StringCell
                {
                    Value = statistics.Name
                },
                new StringCell
                {
                    Value = statistics.Entries.ToString()
                },
                new StringCell
                {
                    Value = statistics.LastBuildTimes.ToString()
                },
                new StringCell
                {
                    Value = statistics.UpdatedAt.ToString()
                },
                new ActionCell
                {
                    Actions = new List<Action>
                    {
                        new Action(ActionType.Command)
                        {
                            Title = "Build index",
                            Label = "Build index",
                            Icon = Icons.RotateRight,
                            Parameter = nameof(Rebuild)
                        },
                        new Action(ActionType.Command)
                        {
                            Title = "Edit",
                            Label = "Edit",
                            Parameter = nameof(Edit),
                            Icon = Icons.Edit
                        },
                        new Action(ActionType.Command)
                        {
                            Title = "Delete",
                            Label = "Delete",
                            Parameter = nameof(Delete),
                            Icon = Icons.Bin
                        }
                    }
                }
            }
        };
    }


    private static IEnumerable<IndicesResponse> SortStatistics(IEnumerable<IndicesResponse> statistics, LoadDataSettings settings)
    {
        if (string.IsNullOrEmpty(settings.SortBy))
        {
            return statistics;
        }

        return settings.SortType == SortTypeEnum.Desc
            ? statistics.OrderByDescending(stat => stat.GetType().GetProperty(settings.SortBy)?.GetValue(stat, null))
            : statistics.OrderBy(stat => stat.GetType().GetProperty(settings.SortBy)?.GetValue(stat, null));
    }

    private async Task<UIPermissions> GetUIPermissions()
    {
        var permissions = new UIPermissions
        {
            Create = (await permissionEvaluator.Evaluate(SystemPermissions.CREATE)).Succeeded,
            Delete = (await permissionEvaluator.Evaluate(SystemPermissions.DELETE)).Succeeded,
            Update = (await permissionEvaluator.Evaluate(SystemPermissions.UPDATE)).Succeeded,
            Rebuild = (await permissionEvaluator.Evaluate(AlgoliaIndexPermissions.REBUILD)).Succeeded,
            View = (await permissionEvaluator.Evaluate(SystemPermissions.VIEW)).Succeeded
        };

        return permissions;
    }
}

internal record struct UIPermissions
{
    public bool View { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public bool Rebuild { get; set; }
    public bool Create { get; set; }
}
