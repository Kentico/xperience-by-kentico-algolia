using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Tests;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

using Tests.DocumentEngine;

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
            IndexStore.Instance
                .Add(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)))
                .Add(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)))
                .Add(new AlgoliaIndex(typeof(TestSearchModels.SplittingModel), nameof(TestSearchModels.SplittingModel),
                    new DistinctOptions(nameof(TestSearchModels.SplittingModel.AttributeForDistinct), 1)));
        }


        [TearDown]
        public void TearDown()
        {
            IndexStore.Instance.Clear();
        }
    }
}
