﻿using System.Collections.Generic;

using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    internal class PathDetailPageProps : TemplateClientProperties
    {
        public string AliasPath
        {
            get;
            set;
        }


        public IEnumerable<Column> Columns
        {
            get;
            set;
        }
    }
}
