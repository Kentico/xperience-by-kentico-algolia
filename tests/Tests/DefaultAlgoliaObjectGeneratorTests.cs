using CMS.Core;
using CMS.DataEngine;
using CMS.MediaLibrary;

using Kentico.Content.Web.Mvc;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;

using NSubstitute;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class DefaultAlgoliaObjectGeneratorTests
    {
        [TestFixture]
        internal class GetTreeNodeDataTests : AlgoliaTests
        {
            private IAlgoliaObjectGenerator algoliaObjectGenerator;


            [SetUp]
            public void GetTreeNodeDataTestsSetUp()
            {
                algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                    new MockEventLogService(),
                    Substitute.For<IMediaFileInfoProvider>(),
                    Substitute.For<IMediaFileUrlRetriever>());
            }


            [Test]
            public void GetTreeNodeData_CustomIndexing_ConvertedToUpper()
            {
                var queueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var articleEnData = algoliaObjectGenerator.GetTreeNodeData(queueItem);

                Assert.That(articleEnData.Value<string>("DocumentName"), Is.EqualTo(FakeNodes.ArticleEn.DocumentName.ToUpper()));
            }


            [Test]
            public void GetTreeNodeData_PartialUpdate_ContainsUpdatedColumns()
            {
                var queueItem = new AlgoliaQueueItem(
                    FakeNodes.ArticleEn,
                    AlgoliaTaskType.UPDATE,
                    nameof(ArticleEnSearchModel),
                    new string[] { nameof(ArticleEnSearchModel.NodeAliasPath) });
                var articleEnData = algoliaObjectGenerator.GetTreeNodeData(queueItem);

                Assert.Multiple(() => {
                    Assert.That(articleEnData.Value<string>("NodeAliasPath"), Is.EqualTo(FakeNodes.ArticleEn.NodeAliasPath));
                    Assert.That(articleEnData.Value<string>("DocumentName"), Is.Null);
                });
            }


            [Test]
            public void GetTreeNodeData_ValidIndex_GetsData()
            {
                var queueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var articleEnData = algoliaObjectGenerator.GetTreeNodeData(queueItem);

                Assert.Multiple(() => {
                    Assert.That(articleEnData.Value<string>("NodeAliasPath"), Is.EqualTo(FakeNodes.ArticleEn.NodeAliasPath));
                    Assert.That(articleEnData.Value<string>("ClassName"), Is.EqualTo(FakeNodes.DOCTYPE_ARTICLE));
                    Assert.That(articleEnData.Value<int>("objectID"), Is.EqualTo(FakeNodes.ArticleEn.DocumentID));
                });
            }
        }
    }
}
