using System;
using System.Linq;
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
        private IncludedPathAttribute pathToDisplay;


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
        /// The internal <see cref="IncludedPathAttribute.Identifier"/> of the indexed path to display the details of.
        /// </summary>
        [PageParameter(typeof(StringPageModelBinder))]
        public string PathIdentifier
        {
            get;
            set;
        }


        /// <inheritdoc/>
        public override Task<PathDetailPageClientProperties> ConfigureTemplateProperties(PathDetailPageClientProperties properties)
        {
            properties.AliasPath = pathToDisplay.AliasPath;
            properties.Columns = new Column[] {
                new Column
                {
                    Caption = LocalizationService.GetString("integrations.algolia.pathdetail.columns.codename"),
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
                var includedPageTypes = pathToDisplay.PageTypes;
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

                return Task.FromResult(ResponseFrom(new LoadDataResult() { Rows = rows, TotalCount = includedPageTypes.Length }));
            }
            catch (Exception ex)
            {
                EventLogService.LogException(nameof(PathDetail), nameof(LoadData), ex);

                return Task.FromResult(ResponseFrom(new LoadDataResult())
                    .AddErrorMessage(LocalizationService.GetString("integrations.algolia.pathdetail.messages.loaderror")));
            }
        }


        /// <inheritdoc/>
        public override Task<PageValidationResult> ValidatePage()
        {
            var index = IndexStore.Instance.GetIndex(IndexIdentifier);
            if (index == null)
            {
                return Task.FromResult(new PageValidationResult {
                    IsValid = false,
                    ErrorMessageKey = "integrations.algolia.error.noindex",
                    ErrorMessageParams = new object[] { IndexIdentifier }
                });
            }

            pathToDisplay = index.IncludedPaths.SingleOrDefault(attr => attr.Identifier.Equals(PathIdentifier, StringComparison.OrdinalIgnoreCase));
            if (pathToDisplay == null)
            {
                return Task.FromResult(new PageValidationResult
                {
                    IsValid = false,
                    ErrorMessageKey = "integrations.algolia.error.nopath",
                    ErrorMessageParams = new object[] { PathIdentifier }
                });
            }

            return base.ValidatePage();
        }
    }
}
