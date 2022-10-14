using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Tests;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

using Tests.DocumentEngine;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal abstract class AlgoliaTests : UnitTests
    {
        [SetUp]
        public void SetUp()
        {
            // Register sites and document types for faking
            DocumentGenerator.RegisterDocumentType<TreeNode>(FakeNodes.DOCTYPE_ARTICLE);
            DocumentGenerator.RegisterDocumentType<TreeNode>(FakeNodes.DOCTYPE_PRODUCT);
            Fake().DocumentType<TreeNode>(FakeNodes.DOCTYPE_ARTICLE);
            Fake().DocumentType<TreeNode>(FakeNodes.DOCTYPE_PRODUCT);
            Fake<SiteInfo, SiteInfoProvider>().WithData(
                new SiteInfo
                {
                    SiteName = FakeNodes.DEFAULT_SITE,
                    SiteDomainName = "defaultsite.com"
                },
                new SiteInfo
                {
                    SiteName = FakeNodes.FAKE_SITE,
                    SiteDomainName = "fakesite.com"
                });

            // Register indexes
            IndexStore.Instance.AddCrawler(CRAWLER_ID);
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(ArticleEnSearchModel), nameof(ArticleEnSearchModel)));
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(ProductsSearchModel), nameof(ProductsSearchModel)));
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(SplittingModel), nameof(SplittingModel),
                    new DistinctOptions(nameof(SplittingModel.AttributeForDistinct), 1)));
        }


        [TearDown]
        public void TearDown()
        {
            IndexStore.Instance.ClearIndexes();
            IndexStore.Instance.ClearCrawlers();
        }
    }
}
