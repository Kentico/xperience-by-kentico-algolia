using System;
using System.Linq;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

namespace Kentico.Xperience.Algolia.Test
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

                Assert.That(IndexStore.Instance.GetAll().Count(), Is.EqualTo(2));
            }
        }


        [TestFixture]
        internal class GetTests : AlgoliaTest
        {
            [Test]
            public void Get_InvalidIndex_ReturnsNull()
            {
                Assert.That(IndexStore.Instance.Get("NO_INDEX"), Is.Null);
            }


            [Test]
            public void Get_ValidIndex_ReturnsIndex()
            {
                var index = IndexStore.Instance.Get(nameof(TestSearchModels.ArticleEnSearchModel));

                Assert.That(index.IndexName, Is.EqualTo(nameof(TestSearchModels.ArticleEnSearchModel)));
            }
        }


        [TestFixture]
        internal class GetAllTests : AlgoliaTest
        {
            [Test]
            public void GetAll_ReturnsAllIndexes()
            {
                Assert.That(IndexStore.Instance.GetAll().Count(), Is.EqualTo(3));
            }
        }
    }
}
