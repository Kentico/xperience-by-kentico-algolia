using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.SiteProvider;

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
            private readonly Guid mediaFileGuid = Guid.NewGuid();
            private readonly IMediaFileInfoProvider mediaFileInfoProvider = Substitute.For<IMediaFileInfoProvider>();
            private readonly IMediaFileUrlRetriever mediaFileUrlRetriever = Substitute.For<IMediaFileUrlRetriever>();


            [SetUp]
            public void GetTreeNodeDataSetUp()
            {
                ModuleManager.GetModule(ModuleName.MEDIALIBRARY).Init();
                
                var site = SiteInfo.Provider.Get(FakeNodes.DEFAULT_SITE);
                Fake<MediaFileInfo, MediaFileInfoProvider>().WithData(
                    new MediaFileInfo
                    {
                        FileGUID = mediaFileGuid,
                        FileSiteID = site.SiteID
                    });

                mediaFileInfoProvider.Get().ReturnsForAnyArgs(new ObjectQuery<MediaFileInfo>());
                algoliaObjectGenerator = new DefaultAlgoliaObjectGenerator(new ConversionService(),
                    Substitute.For<IEventLogService>(),
                    mediaFileInfoProvider,
                    mediaFileUrlRetriever);
            }


            [Test]
            public void GetTreeNodeData_AssetUrlsRetrieved()
            {
                var assetRelatedItem = new AssetRelatedItem
                {
                    Identifier = mediaFileGuid
                };
                var dataType = DataTypeManager.GetDataType(typeof(IEnumerable<AssetRelatedItem>));
                var mediaDbValue = dataType.ConvertToDbType(new AssetRelatedItem[] { assetRelatedItem }, null, Enumerable.Empty<AssetRelatedItem>());
                var articleWithAsset = TreeNode.New<Article>().With(node =>
                {
                    node.ArticleTeaser = mediaDbValue.ToString();
                });
                var queueItem = new AlgoliaQueueItem(articleWithAsset, AlgoliaTaskType.CREATE, nameof(SplittingModel));
                algoliaObjectGenerator.GetTreeNodeData(queueItem);

                mediaFileUrlRetriever.Received(1).Retrieve(Arg.Is<MediaFileInfo>(file => file.FileGUID.Equals(mediaFileGuid)));
            }


            [Test]
            public void GetTreeNodeData_CreateTask_GetsAllData()
            {
                var queueItem = new AlgoliaQueueItem(FakeNodes.ArticleEn, AlgoliaTaskType.CREATE, nameof(ArticleEnSearchModel));
                var articleEnData = algoliaObjectGenerator.GetTreeNodeData(queueItem);

                Assert.Multiple(() => {
                    Assert.That(articleEnData.Value<string>("NodeAliasPath"), Is.EqualTo(FakeNodes.ArticleEn.NodeAliasPath));
                    Assert.That(articleEnData.Value<string>("ClassName"), Is.EqualTo(Article.CLASS_NAME));
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
