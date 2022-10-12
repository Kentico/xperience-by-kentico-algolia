using System;
using System.Linq;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class IndexStoreTests
    {
        [TestFixture]
        internal class AddCrawlerTests
        {
            [Test]
            public void AddCrawler_DuplicateId_Throws()
            {
                IndexStore.Instance.AddCrawler("A");

                Assert.Throws<InvalidOperationException>(() => IndexStore.Instance.AddCrawler("A"));
            }


            [Test]
            public void AddCrawler_ValidId_StoresCrawler()
            {
                IndexStore.Instance.AddCrawler("A");
                IndexStore.Instance.AddCrawler("B");

                Assert.That(IndexStore.Instance.GetAllCrawlers().Count(), Is.EqualTo(2));
            }


            [TearDown]
            public void AddCrawlerTestsTearDown()
            {
                IndexStore.Instance.ClearCrawlers();
            }
        }


        [TestFixture]
        internal class AddIndexTests
        {
            [Test]
            public void AddIndex_DuplicateIndex_Throws()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel))));
            }


            [Test]
            public void AddIndex_InvalidFacetableAttribute_Throws()
            {
                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.InvalidFacetableModel), nameof(TestSearchModels.InvalidFacetableModel))));
            }


            [Test]
            public void AddIndex_ValidIndex_StoresIndex()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.That(IndexStore.Instance.GetAllIndexes().Count(), Is.EqualTo(2));
            }


            [TearDown]
            public void AddIndexTestsTearDown()
            {
                IndexStore.Instance.ClearIndexes();
            }
        }


        [TestFixture]
        internal class GetAllCrawlerTests
        {
            [Test]
            public void GetAllCrawlers_ReturnsAllCrawlers()
            {
                IndexStore.Instance.AddCrawler("A");
                IndexStore.Instance.AddCrawler("B");

                Assert.That(IndexStore.Instance.GetAllCrawlers().Count(), Is.EqualTo(2));
            }


            [TearDown]
            public void AddIndexTestsTearDown()
            {
                IndexStore.Instance.ClearIndexes();
            }
        }


        [TestFixture]
        internal class GetIndexTests : AlgoliaTests
        {
            [Test]
            public void GetIndex_InvalidIndex_ReturnsNull()
            {
                Assert.That(IndexStore.Instance.GetIndex("NO_INDEX"), Is.Null);
            }


            [Test]
            public void GetIndex_InvalidParameters_ThrowsException()
            {
                Assert.Multiple(() => {
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.GetIndex(null));
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.GetIndex(String.Empty));
                });
            }


            [Test]
            public void GetIndex_ValidIndex_ReturnsIndex()
            {
                var index = IndexStore.Instance.GetIndex(nameof(TestSearchModels.ArticleEnSearchModel));

                Assert.That(index.IndexName, Is.EqualTo(nameof(TestSearchModels.ArticleEnSearchModel)));
            }
        }


        [TestFixture]
        internal class GetAllIndexesTests
        {
            [Test]
            public void GetAllIndexes_ReturnsAllIndexes()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.SplittingModel), nameof(TestSearchModels.SplittingModel)));

                Assert.That(IndexStore.Instance.GetAllIndexes().Count(), Is.EqualTo(3));
            }


            [TearDown]
            public void GetAllIndexesTestsTearDown()
            {
                IndexStore.Instance.ClearIndexes();
            }
        }
    }
}
