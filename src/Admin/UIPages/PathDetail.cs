using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    [UIPageLocation(PageLocationEnum.Dialog)]
    internal class PathDetail : Page
    {
        [PageParameter(typeof(IntPageModelBinder))]
        public int IndexIdentifier
        {
            get;
            set;
        }


        [PageParameter(typeof(StringPageModelBinder))]
        public string AliasPath
        {
            get;
            set;
        }
    }
}
