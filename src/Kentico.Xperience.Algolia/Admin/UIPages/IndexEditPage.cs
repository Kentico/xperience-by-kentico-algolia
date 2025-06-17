using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Algolia.Admin;

[assembly: UIPage(
   parentType: typeof(IndexListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(IndexEditPage),
   name: "Edit index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Algolia.Admin;

[UIEvaluatePermission(SystemPermissions.UPDATE)]
internal class IndexEditPage : BaseIndexEditPage
{
    private AlgoliaConfigurationModel? model = null;

    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }

    public IndexEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 IAlgoliaConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder, storageService) { }

    protected override AlgoliaConfigurationModel Model
    {
        get
        {
            model ??= StorageService.GetIndexDataOrNull(IndexIdentifier) ?? new();

            return model;
        }
    }

    protected override Task<ICommandResponse> ProcessFormData(AlgoliaConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result == IndexModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        _ = result == IndexModificationResult.Success
            ? response.AddSuccessMessage("Index edited")
            : response.AddErrorMessage("Could not update index");

        return Task.FromResult<ICommandResponse>(response);
    }
}
