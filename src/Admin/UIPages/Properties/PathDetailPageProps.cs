using System.Collections.Generic;

using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    /// <summary>
    /// Template properties for the <see cref="PathDetail"/> UI page.
    /// </summary>
    internal class PathDetailPageProps : TemplateClientProperties
    {
        /// <summary>
        /// The alias path being displayed.
        /// </summary>
        public string AliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// The columns to display in the page type table.
        /// </summary>
        public IEnumerable<Column> Columns
        {
            get;
            set;
        }
    }
}
