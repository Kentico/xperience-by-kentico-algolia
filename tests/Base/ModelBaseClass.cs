using System;

using CMS.DocumentEngine;
using CMS.Helpers;

using Kentico.Xperience.Algolia.Attributes;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Test
{
    internal class ModelBaseClass : AlgoliaSearchModel
    {
        [Searchable]
        public string NodeAliasPath { get; set; }


        public override object OnIndexingProperty(TreeNode node, string propertyName, string usedColumn, object foundValue)
        {
            if (propertyName.Equals(nameof(TestSearchModels.ArticleEnSearchModel.DocumentName), StringComparison.OrdinalIgnoreCase))
            {
                return ValidationHelper.GetString(foundValue, String.Empty).ToUpper();
            }

            return foundValue;
        }
    }
}
