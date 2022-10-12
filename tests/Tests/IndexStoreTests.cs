using System;
using System.Linq;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class IndexStoreTests
    {
        [TestFixture]
        internal class AddTests
        {
            [Test]
            public void Add_DuplicateIndex_Throws()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel))));
            }


            [Test]
            public void Add_InvalidFacetableAttribute_Throws()
            {
                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.InvalidFacetableModel), nameof(TestSearchModels.InvalidFacetableModel))));
            }


            [Test]
            public void Add_ValidIndex_StoresIndex()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.That(IndexStore.Instance.GetAllIndexes().Count(), Is.EqualTo(2));
            }


            [TearDown]
            public void AddTestsTearDown()
            {
                IndexStore.Instance.Clear();
            }
        }


        [TestFixture]
        internal class GetTests : AlgoliaTests
        {
            [Test]
            public void Get_InvalidIndex_ReturnsNull()
            {
                Assert.That(IndexStore.Instance.GetIndex("NO_INDEX"), Is.Null);
            }


            [Test]
            public void Get_InvalidParameters_ThrowsException()
            {
                Assert.Multiple(() => {
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.GetIndex(null));
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.GetIndex(String.Empty));
                });
            }


            [Test]
            public void Get_ValidIndex_ReturnsIndex()
            {
                var index = IndexStore.Instance.GetIndex(nameof(TestSearchModels.ArticleEnSearchModel));

                Assert.That(index.IndexName, Is.EqualTo(nameof(TestSearchModels.ArticleEnSearchModel)));
            }
        }


        [TestFixture]
        internal class GetAllTests
        {
            [Test]
            public void GetAll_ReturnsAllIndexes()
            {
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)));
                IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(TestSearchModels.SplittingModel), nameof(TestSearchModels.SplittingModel)));

                Assert.That(IndexStore.Instance.GetAllIndexes().Count(), Is.EqualTo(3));
            }


            [TearDown]
            public void GetAllTestsTearDown()
            {
                IndexStore.Instance.Clear();
            }
        }
    }
}
