using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Algolia.Admin;

[assembly: RegisterObjectType(typeof(AlgoliaReusableContentTypeItemInfo), AlgoliaReusableContentTypeItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Data container class for <see cref="AlgoliaReusableContentTypeItemInfo"/>.
/// </summary>
[Serializable]
public class AlgoliaReusableContentTypeItemInfo : AbstractInfo<AlgoliaReusableContentTypeItemInfo, IInfoProvider<AlgoliaReusableContentTypeItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoalgolia.algoliareusablecontenttypeitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AlgoliaReusableContentTypeItemInfo>), OBJECT_TYPE, "KenticoAlgolia.AlgoliaReusableContentTypeItem", nameof(AlgoliaReusableContentTypeItemId), null, nameof(AlgoliaReusableContentTypeItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AlgoliaReusableContentTypeItemIndexItemId), AlgoliaIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Lucene reusable content type item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaReusableContentTypeItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaReusableContentTypeItemId)), 0);
        set => SetValue(nameof(AlgoliaReusableContentTypeItemId), value);
    }


    /// <summary>
    /// Lucene reusable content type item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AlgoliaReusableContentTypeItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AlgoliaReusableContentTypeItemGuid)), default);
        set => SetValue(nameof(AlgoliaReusableContentTypeItemGuid), value);
    }


    /// <summary>
    /// Reusable content type name.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaReusableContentTypeItemContentTypeName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaReusableContentTypeItemContentTypeName)), String.Empty);
        set => SetValue(nameof(AlgoliaReusableContentTypeItemContentTypeName), value);
    }


    /// <summary>
    /// Lucene index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaReusableContentTypeItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaReusableContentTypeItemIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaReusableContentTypeItemIndexItemId), value);
    }


    /// <summary>
    /// Deletes the object using appropriate provider.
    /// </summary>
    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }


    /// <summary>
    /// Updates the object using appropriate provider.
    /// </summary>
    protected override void SetObject()
    {
        Provider.Set(this);
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaReusableContentTypeItemInfo"/> class.
    /// </summary>
    public AlgoliaReusableContentTypeItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaReusableContentTypeItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaReusableContentTypeItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
