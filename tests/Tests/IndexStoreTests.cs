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
                IndexStore.Instance.Add(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.Add(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel))));
            }


            [Test]
            public void Add_InvalidFacetableAttribute_Throws()
            {
                Assert.Throws<InvalidOperationException>(() =>
                    IndexStore.Instance.Add(new AlgoliaIndex(typeof(TestSearchModels.InvalidFacetableModel), nameof(TestSearchModels.InvalidFacetableModel))));
            }


            [Test]
            public void Add_ValidIndex_StoresIndex()
            {
                IndexStore.Instance.Add(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)));
                IndexStore.Instance.Add(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ProductsSearchModel)));

                Assert.That(IndexStore.Instance.GetAll().Count(), Is.EqualTo(2));
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
                Assert.That(IndexStore.Instance.Get("NO_INDEX"), Is.Null);
            }


            [Test]
            public void Get_InvalidParameters_ThrowsException()
            {
                Assert.Multiple(() => {
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.Get(null));
                    Assert.Throws<ArgumentNullException>(() => IndexStore.Instance.Get(String.Empty));
                });
            }


            [Test]
            public void Get_ValidIndex_ReturnsIndex()
            {
                var index = IndexStore.Instance.Get(nameof(TestSearchModels.ArticleEnSearchModel));

                Assert.That(index.IndexName, Is.EqualTo(nameof(TestSearchModels.ArticleEnSearchModel)));
            }
        }


        [TestFixture]
        internal class GetAllTests
        {
            [Test]
            public void GetAll_ReturnsAllIndexes()
            {
                IndexStore.Instance
                    .Add(new AlgoliaIndex(typeof(TestSearchModels.ArticleEnSearchModel), nameof(TestSearchModels.ArticleEnSearchModel)))
                    .Add(new AlgoliaIndex(typeof(TestSearchModels.ProductsSearchModel), nameof(TestSearchModels.ProductsSearchModel)))
                    .Add(new AlgoliaIndex(typeof(TestSearchModels.SplittingModel), nameof(TestSearchModels.SplittingModel)));

                Assert.That(IndexStore.Instance.GetAll().Count(), Is.EqualTo(3));
            }


            [TearDown]
            public void GetAllTestsTearDown()
            {
                IndexStore.Instance.Clear();
            }
        }
    }
}
