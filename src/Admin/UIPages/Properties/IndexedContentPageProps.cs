﻿using System.Collections.Generic;

using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexedContentPageProps : TemplateClientProperties
    {
        public IEnumerable<Column> PathColumns
        {
            get;
            set;
        }


        public IEnumerable<Row> PathRows {
            get;
            set;
        }


        public IEnumerable<Column> PropertyColumns
        {
            get;
            set;
        }


        public IEnumerable<Row> PropertyRows
        {
            get;
            set;
        }
    }
}
