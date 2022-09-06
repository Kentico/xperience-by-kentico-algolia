using System;
using System.Threading.Tasks;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// A navigation section containing pages for viewing index information.
    /// </summary>
    internal class ViewIndexSection : SecondaryMenuSectionPage
    {
        /// <summary>
        /// The internal <see cref="AlgoliaIndex.Identifier"/> of the index.
        /// </summary>
        [PageParameter(typeof(IntPageModelBinder))]
        public int IndexIdentifier
        {
            get;
            set;
        }


        /// <inheritdoc/>
        public override Task<TemplateClientProperties> ConfigureTemplateProperties(TemplateClientProperties properties)
        {
            var index = IndexStore.Instance.Get(IndexIdentifier);
            if (index == null)
            {
                throw new InvalidOperationException($"Unable to retrieve Algolia index with identifier '{IndexIdentifier}.'");
            }

            properties.Breadcrumbs.Label = index.IndexName;
            properties.Breadcrumbs.IsSignificant = true;
            properties.Navigation.Headline = index.IndexName;
            properties.Navigation.IsSignificant = true;

            return base.ConfigureTemplateProperties(properties);
        }
    }
}
