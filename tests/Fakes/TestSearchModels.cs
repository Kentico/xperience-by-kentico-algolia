using Kentico.Xperience.Algolia.Attributes;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class TestSearchModels
    {
        public const string CRAWLER_ID = "crawler";


        [IncludedPath("/Articles/%", PageTypes = new string[] { FakeNodes.DOCTYPE_ARTICLE })]
        public class ArticleEnSearchModel : BaseSearchModel
        {
            public string DocumentName { get; set; }


            [Facetable(FilterOnly = true)]
            public string FacetableProperty { get; set; }


            [Searchable(Unordered = true)]
            public string UnorderedProperty { get; set; }
        }


        [IncludedPath("/Products/%", PageTypes = new string[] { FakeNodes.DOCTYPE_PRODUCT })]
        public class ProductsSearchModel : BaseSearchModel
        {
            [Retrievable]
            public string RetrievableProperty { get; set; }


            [Searchable(Order = 1)]
            public string Order1Property1 { get; set; }


            [Searchable(Order = 1)]
            public string Order1Property2 { get; set; }


            [Searchable(Order = 2)]
            public string Order2Property { get; set; }
        }


        [IncludedPath("/Articles/%")]
        [IncludedPath("/Products/%")]
        public class SplittingModel : BaseSearchModel
        {
            [Searchable]
            public string AttributeForDistinct { get; set; }
        }


        public class InvalidFacetableModel : BaseSearchModel
        {
            [Facetable(FilterOnly = true, Searchable = true)]
            public string FacetableProperty { get; set; }
        }
    }
}
