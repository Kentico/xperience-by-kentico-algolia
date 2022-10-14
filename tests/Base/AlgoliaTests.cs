using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.Tests;

using Kentico.Xperience.Algolia.Models;

using NUnit.Framework;

using Tests.DocumentEngine;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal abstract class AlgoliaTests : UnitTests
    {
        [SetUp]
        public void SetUp()
        {
            // Register sites and document types for faking
            DocumentGenerator.RegisterDocumentType<Article>(Article.CLASS_NAME);
            DocumentGenerator.RegisterDocumentType<TreeNode>(FakeNodes.DOCTYPE_PRODUCT);
            Fake().DocumentType<Article>(Article.CLASS_NAME, dci =>
                dci.ClassFormDefinition = "<form><field column=\"ArticleID\" columnprecision=\"0\" columntype=\"integer\" guid=\"61a0aa87-d18b-4949-b1d0-d7671fd6f526\" isPK=\"true\"><properties><fieldcaption>ArticleID</fieldcaption></properties></field><field column=\"ArticleTitle\" columnprecision=\"0\" columnsize=\"450\" columntype=\"text\" enabled=\"true\" guid=\"df654142-a892-4806-868f-b78c22f1c2b8\" visible=\"true\"><properties><fieldcaption>Title</fieldcaption></properties><settings><controlname>Kentico.Administration.TextInput</controlname></settings></field><field allowempty=\"true\" column=\"ArticleTeaser\" columnprecision=\"0\" columntype=\"assets\" enabled=\"true\" guid=\"decc0681-142f-488f-9e8a-0e55b09e3ba8\" visible=\"true\"><properties><fieldcaption>Teaser</fieldcaption></properties><settings><AllowedExtensions>_INHERITED_</AllowedExtensions><controlname>Kentico.Administration.AssetSelector</controlname><MaximumAssets>1</MaximumAssets></settings></field><field column=\"ArticleSummary\" columnprecision=\"0\" columnsize=\"190\" columntype=\"text\" enabled=\"true\" guid=\"86ca0bd7-d483-45fd-a902-fe3a261d4a45\" visible=\"true\"><properties><fieldcaption>Summary</fieldcaption></properties><settings><controlname>Kentico.Administration.TextArea</controlname><CopyButtonVisible>False</CopyButtonVisible></settings></field><field column=\"ArticleText\" columnprecision=\"0\" columntype=\"longtext\" enabled=\"true\" guid=\"85787b75-3609-48e6-8584-1d88b4a85499\" visible=\"true\"><properties><fieldcaption>Text</fieldcaption></properties><settings><controlname>Kentico.Administration.RichTextEditor</controlname></settings></field></form>");
            Fake().DocumentType<TreeNode>(FakeNodes.DOCTYPE_PRODUCT);
            Fake<SiteInfo, SiteInfoProvider>().WithData(
                new SiteInfo
                {
                    SiteName = FakeNodes.DEFAULT_SITE
                },
                new SiteInfo
                {
                    SiteName = FakeNodes.FAKE_SITE
                });

            // Register indexes
            IndexStore.Instance.AddCrawler(CRAWLER_ID);
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(ArticleEnSearchModel), nameof(ArticleEnSearchModel)));
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(ProductsSearchModel), nameof(ProductsSearchModel)));
            IndexStore.Instance.AddIndex(new AlgoliaIndex(typeof(SplittingModel), nameof(SplittingModel),
                    new DistinctOptions(nameof(SplittingModel.AttributeForDistinct), 1)));
        }


        [TearDown]
        public void TearDown()
        {
            IndexStore.Instance.ClearIndexes();
            IndexStore.Instance.ClearCrawlers();
        }
    }
}
