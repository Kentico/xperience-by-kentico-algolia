using Kentico.Xperience.Algolia.Models;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.Algolia.Admin;

internal class AlgoliaModuleInstaller
{
    private readonly IResourceInfoProvider resourceProvider;

    public AlgoliaModuleInstaller(IResourceInfoProvider resourceProvider) =>
        this.resourceProvider = resourceProvider;

    public void Install()
    {
        var resource = InstallResource();

        InstallAlgoliaItemInfo(resource);
        InstallAlgoliaLanguageItemInfo(resource);
        InstallAlgoliaIndexPathItemInfo(resource);
        InstallAlgoliaContentTypeItemInfo(resource);
    }

    private ResourceInfo InstallResource()
    {
        var resourceInfo = resourceProvider.Get("CMS.Integration.Algolia") ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = "Algolia Search";
        resourceInfo.ResourceName = "Kentico.Xperience.Algolia";
        resourceInfo.ResourceDescription = "Kentico Algolia custom data";
        resourceInfo.ResourceIsInDevelopment = false;
        if (resourceInfo.HasChanged)
        {
            resourceProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallAlgoliaItemInfo(ResourceInfo resource)
    {
        var algoliaItemInfo = DataClassInfoProvider.GetDataClassInfo(AlgoliaIndexItemInfo.OBJECT_TYPE);
        if (algoliaItemInfo is not null)
            return;

        algoliaItemInfo = DataClassInfo.New(AlgoliaIndexItemInfo.OBJECT_TYPE);
        algoliaItemInfo.ClassName = AlgoliaIndexItemInfo.TYPEINFO.ObjectClassName;
        algoliaItemInfo.ClassTableName = AlgoliaIndexItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        algoliaItemInfo.ClassDisplayName = "Algolia Index Item";
        algoliaItemInfo.ClassType = ClassType.OTHER;
        algoliaItemInfo.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true
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

        algoliaItemInfo.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(algoliaItemInfo);
    }

    private void InstallAlgoliaIndexPathItemInfo(ResourceInfo resource)
    {
        var pathItem = DataClassInfoProvider.GetDataClassInfo(AlgoliaIncludedPathItemInfo.OBJECT_TYPE);

        if (pathItem is not null)
            return;

        pathItem = DataClassInfo.New(AlgoliaIncludedPathItemInfo.OBJECT_TYPE);
        pathItem.ClassName = AlgoliaIncludedPathItemInfo.TYPEINFO.ObjectClassName;
        pathItem.ClassTableName = AlgoliaIncludedPathItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        pathItem.ClassDisplayName = "Algolia Path Item";
        pathItem.ClassType = ClassType.OTHER;
        pathItem.ClassResourceID = resource.ResourceID;

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
            Name = nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathAliasPath),
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
            Name = nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        pathItem.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(pathItem);
    }

    private void InstallAlgoliaLanguageItemInfo(ResourceInfo resource)
    {
        var language = DataClassInfoProvider.GetDataClassInfo(AlgoliaIndexLanguageItemInfo.OBJECT_TYPE);

        if (language is not null)
            return;

        language = DataClassInfo.New(AlgoliaIndexLanguageItemInfo.OBJECT_TYPE);
        language.ClassName = AlgoliaIndexLanguageItemInfo.TYPEINFO.ObjectClassName;
        language.ClassTableName = AlgoliaIndexLanguageItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        language.ClassDisplayName = "Algolia Index Language Item";
        language.ClassType = ClassType.OTHER;
        language.ClassResourceID = resource.ResourceID;

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
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        language.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(language);
    }

    private void InstallAlgoliaContentTypeItemInfo(ResourceInfo resource)
    {
        var contentType = DataClassInfoProvider.GetDataClassInfo(AlgoliaContentTypeItemInfo.OBJECT_TYPE);

        if (contentType is not null)
            return;

        contentType = DataClassInfo.New(AlgoliaContentTypeItemInfo.OBJECT_TYPE);
        contentType.ClassName = AlgoliaContentTypeItemInfo.TYPEINFO.ObjectClassName;
        contentType.ClassTableName = AlgoliaContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        contentType.ClassDisplayName = "Algolia Type Item";
        contentType.ClassType = ClassType.OTHER;
        contentType.ClassResourceID = resource.ResourceID;

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
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaIncludedPathItemInfo),
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
            DataType = "integer",
            ReferenceToObjectType = AlgoliaIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        contentType.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(contentType);
    }
}
