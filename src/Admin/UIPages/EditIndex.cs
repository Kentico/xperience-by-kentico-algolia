using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Algolia.Models;
using Kentico.Xperience.Algolia.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin.UIPages;

public class EditIndex : ModelEditPage<AlgoliaConfigurationModel>
{
    [PageParameter(typeof(IntPageModelBinder))]
    public int IndexIdentifier { get; set; }


    private AlgoliaConfigurationModel model;
    private readonly IConfigurationStorageService storageService;


    public EditIndex(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
             IFormDataBinder formDataBinder,
             IConfigurationStorageService storageService)
    : base(formItemCollectionProvider, formDataBinder)
    {
        model = null;
        this.storageService = storageService;
    }

    protected override AlgoliaConfigurationModel Model
    {
        get
        {
            if (model is null)
            {
                if (IndexIdentifier == -1)
                {
                    model = new AlgoliaConfigurationModel();
                }
                else
                {
                    model = storageService.GetIndexDataOrNull(IndexIdentifier).Result ?? new AlgoliaConfigurationModel();
                }
            }
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


    protected override async Task<ICommandResponse> ProcessFormData(AlgoliaConfigurationModel model, ICollection<IFormItem> formItems)
    {
        model.IndexName = RemoveWhitespacesUsingStringBuilder(model.IndexName ?? "");

        if ((await storageService.GetIndexIds()).Any(x => x == model.Id))
        {
            bool edited = await storageService.TryEditIndex(model);

            var response = ResponseFrom(new FormSubmissionResult(edited
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (edited)
            {
                response.AddSuccessMessage("Index edited");

                await AlgoliaSearchModule.AddRegisteredIndices();
            }
            else
            {
                response.AddErrorMessage(string.Format("Editing failed."));
            }

            return response;
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
                created = await storageService.TryCreateIndex(model);
            }

            var response = ResponseFrom(new FormSubmissionResult(created
                                                            ? FormSubmissionStatus.ValidationSuccess
                                                            : FormSubmissionStatus.ValidationFailure));

            if (created)
            {
                response.AddSuccessMessage("Index created");

                model.StrategyName ??= "";

                IndexStore.Instance.AddIndex(new AlgoliaIndex(
                    model.IndexName,
                    model.ChannelName,
                    model.LanguageNames.ToList(),
                    model.Id,
                    model.Paths ?? new List<IncludedPath>(),
                    (IAlgoliaIndexingStrategy)Activator.CreateInstance(StrategyStorage.Strategies[model.StrategyName])
                ));
            }
            else
            {
                response.AddErrorMessage(string.Format("Index creating failed."));
            }

            return response;
        }
    }
}
