using Kentico.Xperience.Admin.Base;

namespace Kentico.Xperience.Algolia.Admin
{
    internal class AlgoliaAdminModule : AdminModule
    {
        public AlgoliaAdminModule()
            : base(nameof(AlgoliaAdminModule))
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            RegisterClientModule("algolia", "web-admin");
        }
    }
}
