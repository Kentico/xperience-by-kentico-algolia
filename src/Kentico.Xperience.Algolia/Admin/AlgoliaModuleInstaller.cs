using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.Algolia.Admin;

internal class AlgoliaModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceProvider;

    public AlgoliaModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider) => this.resourceProvider = resourceProvider;

    public void Install()
    {
        var resource = resourceProvider.Get("CMS.Integration.Algolia")
            ?? new ResourceInfo();

        InitializeResource(resource);
        InstallAlgoliaItemInfo(resource);
        InstallAlgoliaLanguageInfo(resource);
        InstallAlgoliaIndexPathItemInfo(resource);
        InstallAlgoliaContentTypeItemInfo(resource);
        InstallLuceneReusableContentTypeItemInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = "Kentico Integration - Algolia";

        // Prefix ResourceName with "CMS" to prevent C# class generation
        // Classes are already available through the library itself
        resource.ResourceName = "CMS.Integration.Algolia";
        resource.ResourceDescription = "Kentico Algolia custom data";
        resource.ResourceIsInDevelopment = false;
        if (resource.HasChanged)
        {
            resourceProvider.Set(resource);
        }

        return resource;
    }

    public void InstallAlgoliaItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AlgoliaIndexItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AlgoliaIndexItemInfo.OBJECT_TYPE);

        info.ClassName = AlgoliaIndexItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AlgoliaIndexItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Algolia Index Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemChannelName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemStrategyName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemRebuildHook),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAlgoliaIndexPathItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AlgoliaIncludedPathItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AlgoliaIncludedPathItemInfo.OBJECT_TYPE);

        info.ClassName = AlgoliaIncludedPathItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AlgoliaIncludedPathItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Algolia Path Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemAliasPath),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAlgoliaLanguageInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AlgoliaIndexLanguageItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AlgoliaIndexLanguageItemInfo.OBJECT_TYPE);

        info.ClassName = AlgoliaIndexLanguageItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AlgoliaIndexLanguageItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Algolia Indexed Language Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexLanguageItemInfo.AlgoliaIndexLanguageItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallAlgoliaContentTypeItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AlgoliaContentTypeItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AlgoliaContentTypeItemInfo.OBJECT_TYPE);

        info.ClassName = AlgoliaContentTypeItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AlgoliaContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Algolia Type Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemContentTypeName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIncludedPathItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIncludedPathItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallLuceneReusableContentTypeItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(AlgoliaReusableContentTypeItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(AlgoliaReusableContentTypeItemInfo.OBJECT_TYPE);

        info.ClassName = AlgoliaReusableContentTypeItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = AlgoliaReusableContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Algolia Reusable Content Type Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemContentTypeName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaReusableContentTypeItemInfo.AlgoliaReusableContentTypeItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
