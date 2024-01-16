using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Algolia.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kentico.Xperience.Algolia.Indexing;

[assembly: UIPage(
   parentType: typeof(IndexListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(IndexEditPage),
   name: "Edit index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.First)]

namespace Kentico.Xperience.Algolia.Admin;

public class IndexEditPage : ModelEditPage<AlgoliaConfigurationModel>
{
    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }


    private AlgoliaConfigurationModel? model;
    private readonly IAlgoliaConfigurationStorageService storageService;


    public IndexEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
             IFormDataBinder formDataBinder,
             IAlgoliaConfigurationStorageService storageService)
    : base(formItemCollectionProvider, formDataBinder)
    {
        model = null;
        this.storageService = storageService;
    }

    protected override AlgoliaConfigurationModel Model
    {
        get
        {
            model ??= IndexIdentifier == -1
                ? new AlgoliaConfigurationModel()
                : storageService.GetIndexDataOrNull(IndexIdentifier) ?? new AlgoliaConfigurationModel();
            return model;
        }
    }

    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
                builder.Append(c);
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }


    protected override Task<ICommandResponse> ProcessFormData(AlgoliaConfigurationModel model, ICollection<IFormItem> formItems)
    {
        model.IndexName = RemoveWhitespacesUsingStringBuilder(model.IndexName ?? "");

        if ((storageService.GetIndexIds()).Any(x => x == model.Id))
        {
            bool edited = storageService.TryEditIndex(model);

            var response = ResponseFrom(new FormSubmissionResult(edited
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (edited)
            {
                response.AddSuccessMessage("Index edited");

                AlgoliaSearchModule.AddRegisteredIndices();
            }
            else
            {
                response.AddErrorMessage(string.Format("Editing failed."));
            }

            return Task.FromResult<ICommandResponse>(response);
        }
        else
        {
            bool created;
            if (string.IsNullOrWhiteSpace(model.IndexName))
            {
                Response().AddErrorMessage(string.Format("Invalid Index Name"));
                created = false;
            }
            else
            {
                created = storageService.TryCreateIndex(model);
            }

            var response = ResponseFrom(new FormSubmissionResult(created
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (created)
            {
                response.AddSuccessMessage("Index created");

                model.StrategyName ??= "";

                AlgoliaIndexStore.Instance.AddIndex(new AlgoliaIndex(
                     model.IndexName ?? "",
                     model.ChannelName ?? "",
                     model.LanguageNames?.ToList() ?? new(),
                     model.Id,
                     model.Paths ?? new List<AlgoliaIndexIncludedPath>(),
                     luceneIndexingStrategyType: StrategyStorage.Strategies[model.StrategyName] ?? typeof(DefaultAlgoliaIndexingStrategy)
                 ));
            }
            else
            {
                response.AddErrorMessage(string.Format("Index creating failed."));
            }

            return Task.FromResult<ICommandResponse>(response);
        }
    }
}
