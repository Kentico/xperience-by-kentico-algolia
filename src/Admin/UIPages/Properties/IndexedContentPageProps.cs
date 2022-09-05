using System.Collections.Generic;

using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    internal class IndexedContentPageProps : TemplateClientProperties
    {
        public List<Column> PathColumns
        {
            get;
            set;
        } = new();


        public List<Row> PathRows {
            get;
            set;
        }= new();


        public List<Column> PropertyColumns
        {
            get;
            set;
        } = new();


        public List<Row> PropertyRows
        {
            get;
            set;
        } = new();
    }
}
