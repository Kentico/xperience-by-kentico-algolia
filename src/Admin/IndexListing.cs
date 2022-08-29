using Algolia.Search.Models.Common;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Action = Kentico.Xperience.Admin.Base.Action;

[assembly: UIPage(typeof(AlgoliaApplication), "Indexes", typeof(IndexListing), "List of indexes", TemplateNames.LISTING, UIPageOrder.First)]
namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexListing : ListingPage
    {
        private readonly IAlgoliaClient algoliaClient;


        protected override string IdColumn => String.Empty;


        protected override string ObjectType => String.Empty;


        public IndexListing(IAlgoliaClient algoliaClient)
        {
            this.algoliaClient = algoliaClient;
        }


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

            return base.ConfigurePage();
        }


        [PageCommand]
        public ICommandResponse<RowActionResult> RowClick(int id)
        {
            return ResponseFrom(new RowActionResult(false));
        }


        [PageCommand]
        public ICommandResponse<RowActionResult> Rebuild(int id)
        {
            return ResponseFrom(new RowActionResult(false));
        }


        protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
        {
            var statistics = await algoliaClient.GetStatistics();

            // Add statistics for indexes that are registered but not created in Algolia
            AddMissingStatistics(statistics);

            // Remove statistics for indexes that are not registered in this instance
            var filteredStatistics = statistics.Where(stat =>
                IndexStore.Instance.GetAll().Any(index => index.IndexName.Equals(stat.Name, StringComparison.OrdinalIgnoreCase)));

            var searchedStatistics = DoSearch(filteredStatistics, settings.SearchTerm);
            var orderedStatistics = SortStatistics(searchedStatistics, settings);
            var rows = orderedStatistics.Select((stat, i) => GetRow(stat, i));

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
            if (String.IsNullOrEmpty(searchTerm))
            {
                return statistics;
            }

            return statistics.Where(stat => stat.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }


        private Row GetRow(IndicesResponse statistics, int rowNum)
        {
            return new Row
            {
                Identifier = rowNum,
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
                                }
                            }
                        }
                    }
            };
        }


        private IOrderedEnumerable<IndicesResponse> SortStatistics(IEnumerable<IndicesResponse> statistics, LoadDataSettings settings)
        {
            if (String.IsNullOrEmpty(settings.SortBy))
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
