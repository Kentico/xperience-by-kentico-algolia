using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Models.Common;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using Action = Kentico.Xperience.Admin.Base.Action;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// An admin UI page that displays statistics about the registered Algolia indexes.
    /// </summary>
    internal class IndexListing : ListingPageBase<InfoObjectListingConfiguration>
    {
        private readonly IAlgoliaClient algoliaClient;
        private readonly IPageUrlGenerator pageUrlGenerator;


        /// <inheritdoc/>
        public override InfoObjectListingConfiguration PageConfiguration { get; set; } = new InfoObjectListingConfiguration()
        {
            ColumnConfigurations = new List<ColumnConfiguration>(),
            TableActions = new List<ActionConfiguration>(),
            HeaderActions = new List<ActionConfiguration>(),
            PageSizes = new List<int> { 10, 25 },
            QueryModifiers = new List<QueryModifier>()
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
                        Headline = "No indexes",
                        Content = "No Algolia indexes registered. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-algolia'>our instructions</a> to read more about creating and registering Algolia indexes.",
                        ContentAsHtml = true,
                        Type = CalloutType.FriendlyWarning,
                        Placement = CalloutPlacement.OnDesk
                    }
                };
            }

            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(IndicesResponse.Name), "Name", defaultSortDirection: SortTypeEnum.Asc, searchable: true)
                .AddColumn(nameof(IndicesResponse.Entries), "Indexed items")
                .AddColumn(nameof(IndicesResponse.LastBuildTimes), "Build time (seconds)")
                .AddColumn(nameof(IndicesResponse.UpdatedAt), "Last update");

            PageConfiguration.TableActions.AddCommand("Rebuild", nameof(Rebuild), Icons.RotateRight);

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
        [PageCommand]
        public async Task<ICommandResponse<RowActionResult>> Rebuild(int id)
        {
            var result = new RowActionResult(false);
            var index = IndexStore.Instance.Get(id);
            if (index == null)
            {
                return ResponseFrom(result)
                    .AddErrorMessage($"Error loading Algolia index with identifier {id}.");
            }

            await algoliaClient.Rebuild(index.IndexName, CancellationToken.None);

            return ResponseFrom(result)
                .AddSuccessMessage("Indexing in progress. Visit your Algolia dashboard for details about the indexing process.");
        }


        /// <inheritdoc/>
        protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
        {
            var statistics = await algoliaClient.GetStatistics(CancellationToken.None);

            // Add statistics for indexes that are registered but not created in Algolia
            AddMissingStatistics(statistics);

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


        private void AddMissingStatistics(List<IndicesResponse> statistics)
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


        private IEnumerable<IndicesResponse> DoSearch(IEnumerable<IndicesResponse> statistics, string searchTerm)
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
                                    Title = "Rebuild",
                                    Label = "Rebulid",
                                    Icon = Icons.RotateRight,
                                    Parameter = nameof(Rebuild)
                                }
                            }
                        }
                    }
            };
        }


        private IOrderedEnumerable<IndicesResponse> SortStatistics(IEnumerable<IndicesResponse> statistics, LoadDataSettings settings)
        {
            if (string.IsNullOrEmpty(settings.SortBy))
            {
                return statistics.OrderBy(stat => 1);
            }

            switch (settings.SortType)
            {
                case SortTypeEnum.Desc:
                    return statistics.OrderByDescending(stat => stat.GetType().GetProperty(settings.SortBy).GetValue(stat, null));
                default:
                case SortTypeEnum.Asc:
                    return statistics.OrderBy(stat => stat.GetType().GetProperty(settings.SortBy).GetValue(stat, null));
            }
        }
    }
}
