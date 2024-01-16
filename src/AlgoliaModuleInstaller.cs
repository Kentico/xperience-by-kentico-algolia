using Kentico.Xperience.Algolia.Models;
using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;

namespace Kentico.Xperience.Algolia;

public class AlgoliaModuleInstaller
{
    public void Install()
    {
        using (new CMSActionContext { ContinuousIntegrationAllowObjectSerialization = false })
        {
            InstallModuleClasses();
        }
    }

    private void InstallModuleClasses()
    {
        InstallAlgoliaItemInfo();
        InstallAlgoliaLanguageInfo();
        InstallAlgoliaIndexPathItemInfo();
        InstallAlgoliaContentTypeItemInfo();
    }

    private void InstallAlgoliaItemInfo()
    {
        var algoliaItemInfo = DataClassInfoProvider.GetDataClassInfo(AlgoliaIndexItemInfo.OBJECT_TYPE);
        if (algoliaItemInfo is not null)
            return;

        algoliaItemInfo = DataClassInfo.New(AlgoliaIndexItemInfo.OBJECT_TYPE);

        algoliaItemInfo.ClassName = AlgoliaIndexItemInfo.OBJECT_TYPE;
        algoliaItemInfo.ClassTableName = AlgoliaIndexItemInfo.OBJECT_TYPE.Replace(".", "_");
        algoliaItemInfo.ClassDisplayName = "Algolia Index Item";
        algoliaItemInfo.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemId));

        var formItem = new FormFieldInfo
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

    private void InstallAlgoliaIndexPathItemInfo()
    {
        var pathItem = DataClassInfoProvider.GetDataClassInfo(AlgoliaIncludedPathItemInfo.OBJECT_TYPE);

        if (pathItem is not null)
            return;

        pathItem = DataClassInfo.New(AlgoliaIncludedPathItemInfo.OBJECT_TYPE);

        pathItem.ClassName = AlgoliaIncludedPathItemInfo.OBJECT_TYPE;
        pathItem.ClassTableName = AlgoliaIncludedPathItemInfo.OBJECT_TYPE.Replace(".", "_");
        pathItem.ClassDisplayName = "Algolia Path Item";
        pathItem.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaIncludedPathItemInfo.AlgoliaIncludedPathItemId));

        var formItem = new FormFieldInfo
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
            ReferenceToObjectType = nameof(AlgoliaIndexItemInfo),
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        pathItem.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(pathItem);
    }

    private void InstallAlgoliaLanguageInfo()
    {
        string languageInfoName = AlgoliaIndexedLanguageInfo.OBJECT_TYPE;
        string idName = nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageId);
        var language = DataClassInfoProvider.GetDataClassInfo(languageInfoName);

        if (language is not null)
            return;

        language = DataClassInfo.New();

        language.ClassName = languageInfoName;
        language.ClassTableName = languageInfoName.Replace(".", "_");
        language.ClassDisplayName = "Algolia Indexed Language Item";
        language.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(idName);

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageName),
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
            Name = nameof(AlgoliaIndexedLanguageInfo.AlgoliaIndexedLanguageIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaIndexItemInfo),
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        language.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(language);
    }

    private void InstallAlgoliaContentTypeItemInfo()
    {
        var contentType = DataClassInfoProvider.GetDataClassInfo(AlgoliaContentTypeItemInfo.OBJECT_TYPE);

        if (contentType is not null)
            return;

        contentType = DataClassInfo.New();

        contentType.ClassName = AlgoliaContentTypeItemInfo.OBJECT_TYPE;
        contentType.ClassTableName = AlgoliaContentTypeItemInfo.OBJECT_TYPE.Replace(".", "_");
        contentType.ClassDisplayName = "Algolia Type Item";
        contentType.ClassType = ClassType.OTHER;

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
            Name = nameof(AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaIndexItemInfo),
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        contentType.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(contentType);
    }
}
