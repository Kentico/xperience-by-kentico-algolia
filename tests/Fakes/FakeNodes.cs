using System;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.SiteProvider;

namespace Kentico.Xperience.Algolia.Test
{
    internal static class FakeNodes
    {
        public const string DEFAULT_SITE = "TestSite";
        public const string FAKE_SITE = "FAKE_SITE";
        public const string DOCTYPE_ARTICLE = "Test.Article";
        public const string DOCTYPE_PRODUCT = "Test.Product";


        private static int nodeCount = 0;
        private static TreeNode mArticleEn;
        private static TreeNode mArticleCz;
        private static TreeNode mOtherSite;
        private static TreeNode mProductEn;
        private static TreeNode mUnindexed;


        public static TreeNode ArticleEn
        {
            get
            {
                if (mArticleEn == null)
                {
                    mArticleEn = MakeNode("/Articles/1");
                }

                return mArticleEn;
            }
        }


        public static TreeNode ArticleCz
        {
            get
            {
                if (mArticleCz == null)
                {
                    mArticleCz = MakeNode("/Articles/2", culture: "cs-CZ");
                }

                return mArticleCz;
            }
        }


        public static TreeNode ProductEn
        {
            get
            {
                if (mProductEn == null)
                {
                    mProductEn = MakeNode("/Products/1", DOCTYPE_PRODUCT);
                }

                return mProductEn;
            }
        }


        public static TreeNode ArticleOtherSite
        {
            get
            {
                if (mOtherSite == null)
                {
                    mOtherSite = MakeNode("/Articles/1", site: FAKE_SITE);
                }

                return mOtherSite;
            }
        }


        public static TreeNode Unindexed
        {
            get
            {
                if (mUnindexed == null)
                {
                    mUnindexed = MakeNode("/Unindexed/1");
                }

                return mUnindexed;
            }
        }


        private static TreeNode MakeNode(string nodeAliasPath, string pageType = DOCTYPE_ARTICLE, string culture = "en-US", string site = DEFAULT_SITE)
        {
            nodeCount++;
            var nodeSite = SiteInfo.Provider.Get(site);
            var node = TreeNode.New(pageType).With(p =>
            {
                p.DocumentCulture = culture;
                p.SetValue(nameof(TreeNode.DocumentID), nodeCount);
                p.SetValue(nameof(TreeNode.NodeSiteID), nodeSite.SiteID);
                p.SetValue(nameof(TreeNode.DocumentName), Guid.NewGuid().ToString());
                p.SetValue(nameof(TreeNode.DocumentCreatedWhen), new DateTime(2022, 1, 1));
                p.SetValue(nameof(TreeNode.NodeAliasPath), nodeAliasPath);
            });

            return node;
        }
    }
}
