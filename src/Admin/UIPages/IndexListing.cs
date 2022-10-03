using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Models.Common;

using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Action = Kentico.Xperience.Admin.Base.Action;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// An admin UI page that displays statistics about the registered Algolia indexes.
    /// </summary>
    internal class IndexListing : ListingPageBase<ListingConfiguration>
    {
        private readonly IAlgoliaClient algoliaClient;
        private readonly IPageUrlGenerator pageUrlGenerator;


        /// <inheritdoc/>
        public override ListingConfiguration PageConfiguration { get; set; } = new ListingConfiguration()
        {
            ColumnConfigurations = new List<ColumnConfiguration>(),
            TableActions = new List<ActionConfiguration>(),
            HeaderActions = new List<ActionConfiguration>(),
            PageSizes = new List<int> { 10, 25 }
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="IndexListing"/> class.
        /// </summary>
        public IndexListing(IAlgoliaClient algoliaClient, IPageUrlGenerator pageUrlGenerator)
        {
            this.algoliaClient = algoliaClient;
            this.pageUrlGenerator = pageUrlGenerator;
        }


        /// <inheritdoc/>
        public override Task ConfigurePage()
        {
            if (!IndexStore.Instance.GetAll().Any())
            {
                PageConfiguration.Callouts = new List<CalloutConfiguration>
                {
                    new CalloutConfiguration
                    {
                        Headline = LocalizationService.GetString("integrations.algolia.listing.noindexes.headline"),
                        Content = LocalizationService.GetString("integrations.algolia.listing.noindexes.description"),
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning,
                        Placement = CalloutPlacement.OnDesk
                    }
                };
            }

            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(IndicesResponse.Name), LocalizationService.GetString("integrations.algolia.listing.columns.name"), defaultSortDirection: SortTypeEnum.Asc, searchable: true)
                .AddColumn(nameof(IndicesResponse.Entries), LocalizationService.GetString("integrations.algolia.listing.columns.entries"))
                .AddColumn(nameof(IndicesResponse.LastBuildTimes), LocalizationService.GetString("integrations.algolia.listing.columns.buildtime"))
                .AddColumn(nameof(IndicesResponse.UpdatedAt), LocalizationService.GetString("integrations.algolia.listing.columns.updatedat"));

            PageConfiguration.TableActions.AddCommand(LocalizationService.GetString("integrations.algolia.listing.commands.rebuild"), nameof(Rebuild), Icons.RotateRight);

            return base.ConfigurePage();
        }


        /// <summary>
        /// A page command which displays details about an index.
        /// </summary>
        /// <param name="id">The ID of the row that was clicked, which corresponds with the internal
        /// <see cref="AlgoliaIndex.Identifier"/> to display.</param>
        [PageCommand]
        public Task<INavigateResponse> RowClick(int id)
        {
            return Task.FromResult(NavigateTo(pageUrlGenerator.GenerateUrl(typeof(IndexedContent), id.ToString())));
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
            var index = IndexStore.Instance.Get(id);
            if (index == null)
            {
                return ResponseFrom(result)
                    .AddErrorMessage(String.Format(LocalizationService.GetString("integrations.algolia.error.noindex"), id));
            }

            try
            {
                await algoliaClient.Rebuild(index.IndexName, cancellationToken);
                return ResponseFrom(result)
                    .AddSuccessMessage(LocalizationService.GetString("integrations.algolia.listing.messages.rebuilding"));
            }
            catch(Exception ex)
            {
                EventLogService.LogException(nameof(IndexListing), nameof(Rebuild), ex);
                return ResponseFrom(result)
                    .AddErrorMessage(String.Format(LocalizationService.GetString("integrations.algolia.listing.messages.rebuilderror", index.IndexName)));
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
                    IndexStore.Instance.GetAll().Any(index => index.IndexName.Equals(stat.Name, StringComparison.OrdinalIgnoreCase)));

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
                EventLogService.LogException(nameof(IndexListing), nameof(LoadData), ex);
                return new LoadDataResult
                {
                    Rows = Enumerable.Empty<Row>(),
                    TotalCount = 0
                };
            }
        }


        private void AddMissingStatistics(ref ICollection<IndicesResponse> statistics)
        {
            foreach (var indexName in IndexStore.Instance.GetAll().Select(i => i.IndexName))
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
            var algoliaIdex = IndexStore.Instance.Get(statistics.Name);
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
                                    Title = LocalizationService.GetString("integrations.algolia.listing.commands.rebuild"),
                                    Label = LocalizationService.GetString("integrations.algolia.listing.commands.rebuild"),
                                    Icon = Icons.RotateRight,
                                    Parameter = nameof(Rebuild)
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

            return settings.SortType switch
            {
                SortTypeEnum.Desc => statistics.OrderByDescending(stat => stat.GetType().GetProperty(settings.SortBy).GetValue(stat, null)),
                _ => statistics.OrderBy(stat => stat.GetType().GetProperty(settings.SortBy).GetValue(stat, null)),
            };
        }
    }
}
