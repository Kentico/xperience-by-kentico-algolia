using Kentico.Xperience.Algolia.Extensions;

using NUnit.Framework;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class TreeNodeExtensionsTests
    {
        [TestFixture]
        internal class IsAlgoliaIndexedTests : AlgoliaTests
        {
            [Test]
            public void IsAlgoliaIndexed_ReturnsIndexedState()
            {
                Assert.Multiple(() =>
                {
                    Assert.That(FakeNodes.ArticleEn.IsAlgoliaIndexed(), Is.True);
                    Assert.That(FakeNodes.ProductEn.IsAlgoliaIndexed(), Is.True);
                    Assert.That(FakeNodes.ArticleOtherSite.IsAlgoliaIndexed(), Is.True);
                    Assert.That(FakeNodes.Unindexed.IsAlgoliaIndexed(), Is.False);
                });
            }
        }


        [TestFixture]
        internal class IsIndexedByIndexTests : AlgoliaTests
        {
            [Test]
            public void IsIndexedByIndex_ReturnsIndexedState()
            {
                Assert.Multiple(() =>
                {
                    Assert.That(FakeNodes.ArticleEn.IsIndexedByIndex(nameof(TestSearchModels.ArticleEnSearchModel)), Is.True);
                    Assert.That(FakeNodes.ArticleEn.IsIndexedByIndex(nameof(TestSearchModels.ProductsSearchModel)), Is.False);
                    Assert.That(FakeNodes.ProductEn.IsIndexedByIndex(nameof(TestSearchModels.ArticleEnSearchModel)), Is.False);
                    Assert.That(FakeNodes.ProductEn.IsIndexedByIndex(nameof(TestSearchModels.ProductsSearchModel)), Is.True);
                });
            }
        }
    }
}
