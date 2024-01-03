using CMS;
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
        var algoliaItemInfo = DataClassInfoProvider.GetDataClassInfo(AlgoliaindexitemInfo.OBJECT_TYPE);
        if (algoliaItemInfo is not null)
            return;

        algoliaItemInfo = DataClassInfo.New(AlgoliaindexitemInfo.OBJECT_TYPE);

        algoliaItemInfo.ClassName = AlgoliaindexitemInfo.OBJECT_TYPE;
        algoliaItemInfo.ClassTableName = AlgoliaindexitemInfo.OBJECT_TYPE.Replace(".", "_");
        algoliaItemInfo.ClassDisplayName = "Algolia Index Item";
        algoliaItemInfo.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaindexitemInfo.IndexName),
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
            Name = nameof(AlgoliaindexitemInfo.ChannelName),
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
            Name = nameof(AlgoliaindexitemInfo.StrategyName),
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
            Name = nameof(AlgoliaindexitemInfo.RebuildHook),
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
        var pathItem = DataClassInfoProvider.GetDataClassInfo(AlgoliaincludedpathitemInfo.OBJECT_TYPE);

        if (pathItem is not null)
            return;

        pathItem = DataClassInfo.New(AlgoliaincludedpathitemInfo.OBJECT_TYPE);

        pathItem.ClassName = AlgoliaincludedpathitemInfo.OBJECT_TYPE;
        pathItem.ClassTableName = AlgoliaincludedpathitemInfo.OBJECT_TYPE.Replace(".", "_");
        pathItem.ClassDisplayName = "Algolia Path Item";
        pathItem.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliaincludedpathitemInfo.AlgoliaIncludedPathItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaincludedpathitemInfo.AliasPath),
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
            Name = nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaindexitemInfo),
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        pathItem.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(pathItem);
    }

    private void InstallAlgoliaLanguageInfo()
    {
        string languageInfoName = AlgoliaindexedlanguageInfo.OBJECT_TYPE;
        string idName = nameof(AlgoliaindexedlanguageInfo.IndexedLanguageId);
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
            Name = nameof(AlgoliaindexedlanguageInfo.languageCode),
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
            Name = nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaindexitemInfo),
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        language.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(language);
    }

    private void InstallAlgoliaContentTypeItemInfo()
    {
        var contentType = DataClassInfoProvider.GetDataClassInfo(AlgoliacontenttypeitemInfo.OBJECT_TYPE);

        if (contentType is not null)
            return;

        contentType = DataClassInfo.New();

        contentType.ClassName = AlgoliacontenttypeitemInfo.OBJECT_TYPE;
        contentType.ClassTableName = AlgoliacontenttypeitemInfo.OBJECT_TYPE.Replace(".", "_");
        contentType.ClassDisplayName = "Algolia Type Item";
        contentType.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(AlgoliacontenttypeitemInfo.AlgoliaContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliacontenttypeitemInfo.ContentTypeName),
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
            Name = nameof(AlgoliaincludedpathitemInfo.AlgoliaIncludedPathItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaincludedpathitemInfo),
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(AlgoliaindexitemInfo.AlgoliaIndexItemId),
            AllowEmpty = false,
            Visible = true,
            DataType = "integer",
            ReferenceToObjectType = nameof(AlgoliaindexitemInfo),
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        contentType.ClassFormDefinition = formInfo.GetXmlDefinition();

        DataClassInfoProvider.SetDataClassInfo(contentType);
    }
}
