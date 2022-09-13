using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// An admin UI dialog page which displays the page types included in an indexed path.
    /// </summary>
    [UIBreadcrumbs(false)]
    [UIPageLocation(PageLocationEnum.Dialog)]
    internal class PathDetail : Page<PathDetailPageClientProperties>
    {
        private string mAliasPath;


        /// <summary>
        /// The internal <see cref="AlgoliaIndex.Identifier"/> of the index that contains the
        /// indexed path definition.
        /// </summary>
        [PageParameter(typeof(IntPageModelBinder), typeof(ViewIndexSection))]
        public int IndexIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// The indexed path to display the details of.
        /// </summary>
        [PageParameter(typeof(StringPageModelBinder))]
        public string AliasPath
        {
            get
            {
                return mAliasPath;
            }
            set
            {
                mAliasPath = Uri.UnescapeDataString(value);
            }
        }


        /// <inheritdoc/>
        public override Task<PathDetailPageClientProperties> ConfigureTemplateProperties(PathDetailPageClientProperties properties)
        {
            properties.AliasPath = AliasPath;
            properties.Columns = new Column[] {
                new Column
                {
                    Caption = "Code name",
                    ContentType = ColumnContentType.Text,
                    MinWidth = 80,
                    Visible = true
                }
            };

            return Task.FromResult(properties);
        }


        /// <summary>
        /// Returns the data to display in the page type table and the number of total items.
        /// </summary>
        /// <param name="args">Command arguments provided by client.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [PageCommand]
        public Task<ICommandResponse<LoadDataResult>> LoadData(LoadDataCommandArguments args, CancellationToken cancellationToken)
        {
            try
            {
                var index = IndexStore.Instance.Get(IndexIdentifier);
                if (index == null)
                {
                    throw new InvalidOperationException($"Unable to retrieve Algolia index with identifier '{IndexIdentifier}.'");
                }

                var includedPathAttribute = index.Type.GetCustomAttributes<IncludedPathAttribute>(false)
                    .FirstOrDefault(attr => attr.AliasPath.Equals(AliasPath, StringComparison.OrdinalIgnoreCase));
                if (includedPathAttribute == null)
                {
                    throw new InvalidOperationException($"Unable to load included path definition for alias '{AliasPath}.'");
                }

                var includedPageTypes = includedPathAttribute.PageTypes;
                if (!includedPageTypes.Any())
                {
                    includedPageTypes = DocumentTypeHelper.GetDocumentTypeClasses()
                        .OnSite(SiteService.CurrentSite?.SiteID)
                        .AsSingleColumn(nameof(DataClassInfo.ClassName))
                        .GetListResult<string>()
                        .ToArray();
                }

                var rows = includedPageTypes.Select(type => new Row
                {
                    Cells = new Cell[] {
                    new StringCell
                    {
                        Value = type
                    }
                }
                })
                .Chunk(args.PageSize)
                .ElementAtOrDefault(args.CurrentPage - 1);

                return Task.FromResult(ResponseFrom(new LoadDataResult() { Rows = rows, TotalCount = includedPageTypes.Count() }));
            }
            catch (Exception ex)
            {
                EventLogService.LogException(nameof(PathDetail), nameof(LoadData), ex);

                return Task.FromResult(ResponseFrom(new LoadDataResult()).AddErrorMessage("An error occurred while loading data. Please check the Event Log for more details."));
            }
        }
    }
}
