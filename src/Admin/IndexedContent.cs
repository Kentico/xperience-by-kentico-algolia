using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

[assembly: UIPage(typeof(IndexListing), PageParameterConstants.PARAMETERIZED_SLUG, typeof(IndexedContent), "Indexed content", TemplateNames.LISTING, 1)]
namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexedContent : ListingPageBase<InfoObjectListingConfiguration>
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public int IndexIdentifier {
            get;
            set;
        }


        public override InfoObjectListingConfiguration PageConfiguration { get; set; } = new InfoObjectListingConfiguration()
        {
            ColumnConfigurations = new List<ColumnConfiguration>(),
            TableActions = new List<ActionConfiguration>(),
            HeaderActions = new List<ActionConfiguration>(),
            PageSizes = new List<int> { 10, 25 },
            QueryModifiers = new List<QueryModifier>()
        };


        public override Task ConfigurePage()
        {
            PageConfiguration.ColumnConfigurations
                .AddColumn(nameof(IncludedPathAttribute.AliasPath), "Path", sortable: false)
                .AddComponentColumn(nameof(IncludedPathAttribute.PageTypes), NamedComponentCellComponentNames.TAG_COMPONENT, "Page types", sortable: false);

            return base.ConfigurePage();
        }


        protected override Task<LoadDataResult> LoadData(Xperience.Admin.Base.LoadDataSettings settings, CancellationToken cancellationToken)
        {
            var rows = GetRows();

            return Task.FromResult(new LoadDataResult
            {
                Rows = rows,
                TotalCount = rows.Count()
            });
        }


        private IEnumerable<Row> GetRows()
        {
            var index = IndexStore.Instance.Get(IndexIdentifier);
            if (index == null)
            {
                throw new InvalidOperationException($"Unable to retrieve Algolia index with identifier '{IndexIdentifier}.'");
            }

            var includedPathAttributes = index.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            return includedPathAttributes.Select((attr, i) => GetRow(attr, i));
        }


        private Row GetRow(IncludedPathAttribute attribute, int rowNum)
        {
            string pageTypeLabel;
            Color pageTypeBackground;
            if (!attribute.PageTypes.Any())
            {
                var allTypes = DocumentTypeHelper.GetDocumentTypeClasses()
                    .OnSite(SiteService.CurrentSite?.SiteID)
                    .Columns(nameof(DataClassInfo.ClassIsDocumentType))
                    .Count;
                pageTypeLabel = $"All ({allTypes})";
                pageTypeBackground = Color.BackgroundTagGrey;
            }
            else if (attribute.PageTypes.Count() == 1)
            {
                pageTypeLabel = $"1 page type";
                pageTypeBackground = Color.BackgroundTagMajorelleBlue;
            }
            else
            {
                pageTypeLabel = $"{attribute.PageTypes.Count()} page types";
                pageTypeBackground = Color.BackgroundTagUltramarineBlue;
            }

            return new Row
            {
                Identifier = rowNum,
                Cells = new List<Cell>
                    {
                        new StringCell
                        {
                            Value = attribute.AliasPath
                        },
                        new NamedComponentCell
                        {
                            Name = NamedComponentCellComponentNames.TAG_COMPONENT,
                            ComponentProps = new TagTableCellComponentProps
                            {
                                Label = pageTypeLabel,
                                Color = pageTypeBackground
                            }
                        },
                    }
            };
        }
    }
}
