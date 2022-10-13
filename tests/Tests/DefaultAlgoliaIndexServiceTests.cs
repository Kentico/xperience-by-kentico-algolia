using System;
using System.Threading;

using Algolia.Search.Clients;

using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using NSubstitute;

using NUnit.Framework;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaIndexServiceTests
    {
        [TestFixture]
        internal class GetIndexSettingsTests : AlgoliaTests
        {
            private readonly DefaultAlgoliaIndexService algoliaIndexService = new(Substitute.For<ISearchClient>());


            [TestCase(typeof(TestSearchModels.ArticleEnSearchModel), ExpectedResult = new string[] { "filterOnly(FacetableProperty)", "searchable(ClassName)" })]
            public string[] GetIndexSettings_AttributesForFaceting_PropertiesConvertedToArray(Type searchModel)
            {
                var algoliaIndex = new AlgoliaIndex(searchModel, nameof(searchModel));

                return algoliaIndexService.GetIndexSettings(algoliaIndex).AttributesForFaceting.ToArray();
            }


            [TestCase(typeof(TestSearchModels.ProductsSearchModel), ExpectedResult = new string[] { "RetrievableProperty", "ObjectID", "ClassName", "Url" })]
            public string[] GetIndexSettings_AttributesToRetrieve_PropertiesConvertedToArray(Type searchModel)
            {
                var algoliaIndex = new AlgoliaIndex(searchModel, nameof(searchModel));

                return algoliaIndexService.GetIndexSettings(algoliaIndex).AttributesToRetrieve.ToArray();
            }


            [Test]
            public void GetIndexSettings_DistinctOptions_ReturnsOptions()
            {
                var algoliaIndex = IndexStore.Instance.GetIndex(nameof(TestSearchModels.SplittingModel));
                var indexSettings = algoliaIndexService.GetIndexSettings(algoliaIndex);

                Assert.Multiple(() => {
                    Assert.That(indexSettings.AttributeForDistinct, Is.EqualTo(nameof(TestSearchModels.SplittingModel.AttributeForDistinct)));
                    Assert.That(indexSettings.Distinct, Is.EqualTo(1));
                });
            }


            [TestCase(typeof(TestSearchModels.ArticleEnSearchModel), ExpectedResult = new string[] { "unordered(UnorderedProperty)", "NodeAliasPath" })]
            [TestCase(typeof(TestSearchModels.ProductsSearchModel), ExpectedResult = new string[] { "Order1Property1,Order1Property2", "Order2Property", "NodeAliasPath" })]
            public string[] GetIndexSettings_SearchableAttributes_PropertiesConvertedToArray(Type searchModel)
            {
                var algoliaIndex = new AlgoliaIndex(searchModel, nameof(searchModel));

                return algoliaIndexService.GetIndexSettings(algoliaIndex).SearchableAttributes.ToArray();
            }
        }


        [TestFixture]
        internal class InitializeCrawlerTests : AlgoliaTests
        {
            private readonly DefaultAlgoliaIndexService algoliaIndexService = new(Substitute.For<ISearchClient>());


            [Test]
            public void InitializeCrawler_InvalidConfiguration_Throws()
            {
                Assert.Multiple(() =>
                {
                    Assert.Throws<ArgumentNullException>(() => algoliaIndexService.InitializeCrawler(null));
                    Assert.Throws<InvalidOperationException>(() => algoliaIndexService.InitializeCrawler(new AlgoliaCrawler
                    {
                        Name = String.Empty
                    }));
                });
            }
        }


        [TestFixture]
        internal class InitializeIndexTests : AlgoliaTests
        {
            private readonly DefaultAlgoliaIndexService algoliaIndexService = new(Substitute.For<ISearchClient>());


            [Test]
            public void InitializeIndex_InvalidIndex_Throws()
            {
                Assert.ThrowsAsync<InvalidOperationException>(async() => await algoliaIndexService.InitializeIndex("NO_INDEX", CancellationToken.None));
            }
        }
    }
}
