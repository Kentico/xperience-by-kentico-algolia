using Algolia.Search.Models.Common;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[assembly: UIPage(typeof(AlgoliaApplication), "list", typeof(IndexListing), "List of indexes", TemplateNames.LISTING, UIPageOrder.First)]
namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexListing : ListingPage
    {
        private readonly IAlgoliaClient algoliaClient;


        protected override string IdColumn => "IndexName";


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
                .AddColumn("IndexName", "Name", defaultSortDirection: SortTypeEnum.Asc, searchable: true)
                .AddColumn("Entries", "Indexed items", defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn("BuildTime", "Build time (seconds)", defaultSortDirection: SortTypeEnum.Asc)
                .AddColumn("LastUpdate", "Last update", defaultSortDirection: SortTypeEnum.Asc);

            return base.ConfigurePage();
        }


        protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
        {
            var statistics = await algoliaClient.GetStatistics();
            var rows = IndexStore.Instance.GetAll().Select(index => GetRow(index, statistics));

            return new LoadDataResult
            {
                Rows = rows,
                TotalCount = rows.Count()
            };
        }


        private Row GetRow(AlgoliaIndex algoliaIndex, List<IndicesResponse> statistics)
        {
            var matchingStatistics = statistics.FirstOrDefault(i => i.Name.Equals(algoliaIndex.IndexName, StringComparison.OrdinalIgnoreCase));
            var entries = matchingStatistics?.Entries.ToString() ?? "0";
            var buildTime = matchingStatistics?.LastBuildTimes.ToString() ?? "0";
            var lastUpdate = matchingStatistics?.UpdatedAt.ToString() ?? "N/A";
            return new Row
            {
                Identifier = 1,
                Cells = new List<Cell>
                    {
                        new StringCell
                        {
                            Value = algoliaIndex.IndexName
                        },
                        new StringCell
                        {
                            Value = entries
                        },
                        new StringCell
                        {
                            Value = buildTime
                        },
                        new StringCell
                        {
                            Value = lastUpdate
                        }
                    }
            };
        }
    }
}
