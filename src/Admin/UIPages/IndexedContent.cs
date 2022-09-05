using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CMS.DataEngine;
using CMS.DocumentEngine;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Attributes;

namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexedContent : Page<IndexedContentPageProps>
    {
        private readonly IPageUrlGenerator pageUrlGenerator;


        [PageParameter(typeof(IntPageModelBinder))]
        public int IndexIdentifier
        {
            get;
            set;
        }


        public IndexedContent(IPageUrlGenerator pageUrlGenerator)
        {
            this.pageUrlGenerator = pageUrlGenerator;
        }


        [PageCommand]
        public Task<INavigateResponse> ShowPathDetail(PathDetailArguments args)
        {
            var aliasPath = Uri.EscapeDataString(args.Cell.Value);

            return Task.FromResult(NavigateTo(pageUrlGenerator.GenerateUrl(typeof(PathDetail), IndexIdentifier.ToString(), aliasPath)));
        }


        public override Task<IndexedContentPageProps> ConfigureTemplateProperties(IndexedContentPageProps properties)
        {
            var index = IndexStore.Instance.Get(IndexIdentifier);
            if (index == null)
            {
                throw new InvalidOperationException($"Unable to retrieve Algolia index with identifier '{IndexIdentifier}.'");
            }

            var includedPathAttributes = index.Type.GetCustomAttributes<IncludedPathAttribute>(false);
            properties.PathRows = includedPathAttributes.Select((attr, i) => GetPath(attr, i));
            properties.PathColumns = GetPathColumns();

            var searchModelProperties = index.Type.GetProperties();
            properties.PropertyRows = searchModelProperties.Select(prop => GetProperty(prop));
            properties.PropertyColumns = GetPropertyColumns();

            return Task.FromResult(properties);
        }


        private Row GetPath(IncludedPathAttribute attribute, int rowNum)
        {
            return new Row
            {
                Identifier = rowNum,
                Cells = new Cell[] {
                    new StringCell
                    {
                        Value = attribute.AliasPath
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.TAG_COMPONENT,
                        ComponentProps = new TagTableCellComponentProps
                        {
                            Label = GetPageTypeLabel(attribute),
                            Color = GetPageTypeColor(attribute)
                        }
                    }
                }
            };
        }


        private Column[] GetPathColumns()
        {
            return new Column[] {
                new Column
                {
                    Caption = "Path",
                    ContentType = ColumnContentType.Text
                },
                new Column
                {
                    Caption = "Page types",
                    ContentType = ColumnContentType.Component
                }
            };
        }


        private Row GetProperty(PropertyInfo property)
        {
            var isSearchable = Attribute.IsDefined(property, typeof(SearchableAttribute));
            var isRetrievable = Attribute.IsDefined(property, typeof(RetrievableAttribute));
            var isFacetable = Attribute.IsDefined(property, typeof(FacetableAttribute));
            var hasSources = Attribute.IsDefined(property, typeof(SourceAttribute));
            var hasUrls = Attribute.IsDefined(property, typeof(MediaUrlsAttribute));
            return new Row
            {
                Cells = new Cell[] {
                    new StringCell
                    {
                        Value = property.Name
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT,
                        ComponentProps = new SimpleStatusNamedComponentCellProps
                        {
                            IconName = GetIconName(isSearchable),
                            IconColor = GetIconColor(isSearchable)
                        }
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT,
                        ComponentProps = new SimpleStatusNamedComponentCellProps
                        {
                            IconName = GetIconName(isRetrievable),
                            IconColor = GetIconColor(isRetrievable)
                        }
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT,
                        ComponentProps = new SimpleStatusNamedComponentCellProps
                        {
                            IconName = GetIconName(isFacetable),
                            IconColor = GetIconColor(isFacetable)
                        }
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT,
                        ComponentProps = new SimpleStatusNamedComponentCellProps
                        {
                            IconName = GetIconName(hasSources),
                            IconColor = GetIconColor(hasSources)
                        }
                    },
                    new NamedComponentCell
                    {
                        Name = NamedComponentCellComponentNames.SIMPLE_STATUS_COMPONENT,
                        ComponentProps = new SimpleStatusNamedComponentCellProps
                        {
                            IconName = GetIconName(hasUrls),
                            IconColor = GetIconColor(hasUrls)
                        }
                    }
                }
            };
        }


        private Column[] GetPropertyColumns()
        {
            return new Column[] {
                new Column
                {
                    Caption = "Property",
                    ContentType = ColumnContentType.Text
                },
                new Column
                {
                    Caption = "Searchable",
                    ContentType = ColumnContentType.Component
                },
                new Column
                {
                    Caption = "Retrievable",
                    ContentType = ColumnContentType.Component
                },
                new Column
                {
                    Caption = "Facetable",
                    ContentType = ColumnContentType.Component
                },
                new Column
                {
                    Caption = "Source",
                    ContentType = ColumnContentType.Component
                },
                new Column
                {
                    Caption = "URL",
                    ContentType = ColumnContentType.Component
                }
            };
        }


        private Color GetIconColor(bool status)
        {
            return status ? Color.SuccessIcon : Color.IconLowEmphasis;
        }


        private string GetIconName(bool status)
        {
            return status ? Icons.Check : Icons.Minus;
        }


        private Color GetPageTypeColor(IncludedPathAttribute attribute)
        {
            if (!attribute.PageTypes.Any())
            {
                return Color.BackgroundTagGrey;
            }
            else if (attribute.PageTypes.Count() == 1)
            {
                return Color.BackgroundTagUltramarineBlue;
            }

            return Color.BackgroundTagDefault;
        }


        private string GetPageTypeLabel(IncludedPathAttribute attribute)
        {
            if (!attribute.PageTypes.Any())
            {
                var allTypes = DocumentTypeHelper.GetDocumentTypeClasses()
                    .OnSite(SiteService.CurrentSite?.SiteID)
                    .Columns(nameof(DataClassInfo.ClassIsDocumentType))
                    .Count;
                return $"All ({allTypes})";
            }
            else if (attribute.PageTypes.Count() == 1)
            {
                return "1 page type";
            }

            return $"{attribute.PageTypes.Count()} page types";
        }


        internal class PathDetailArguments
        {
            public CellData Cell { get; set; }
        }


        internal class CellData
        {
            public string Value { get; set; }
        }
    }
}
