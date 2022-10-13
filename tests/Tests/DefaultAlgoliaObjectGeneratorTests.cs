using CMS.Core;
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
            private readonly IAlgoliaObjectGenerator algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(Substitute.For<IConversionService>(),
                Substitute.For<IEventLogService>(),
                Substitute.For<IMediaFileInfoProvider>(),
                Substitute.For<IMediaFileUrlRetriever>());


            [Test]
            public void GetTreeNodeData_CreateTask_GetsAllData()
            {
                var queueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var articleEnData = algoliaObjectGenerator.GetTreeNodeData(queueItem);

                Assert.Multiple(() => {
                    Assert.That(articleEnData.Value<string>("NodeAliasPath"), Is.EqualTo(FakeNodes.ArticleEn.NodeAliasPath));
                    Assert.That(articleEnData.Value<string>("ClassName"), Is.EqualTo(FakeNodes.DOCTYPE_ARTICLE));
                    Assert.That(articleEnData.Value<string>("DocumentName"), Is.EqualTo(FakeNodes.ArticleEn.DocumentName.ToUpper()));
                    Assert.That(articleEnData.Value<int>("objectID"), Is.EqualTo(FakeNodes.ArticleEn.DocumentID));
                });
            }


            [Test]
            public void GetTreeNodeData_UpdateTask_ContainsUpdatedColumns()
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
        }
    }
}
