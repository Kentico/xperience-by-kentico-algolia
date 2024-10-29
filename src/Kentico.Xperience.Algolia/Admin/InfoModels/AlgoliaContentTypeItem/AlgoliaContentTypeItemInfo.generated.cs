
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Algolia.Admin;

[assembly: RegisterObjectType(typeof(AlgoliaContentTypeItemInfo), AlgoliaContentTypeItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Data container class for <see cref="AlgoliaContentTypeItemInfo"/>.
/// </summary>
[Serializable]
public partial class AlgoliaContentTypeItemInfo : AbstractInfo<AlgoliaContentTypeItemInfo, IInfoProvider<AlgoliaContentTypeItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoalgolia.algoliacontenttypeitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AlgoliaContentTypeItemInfo>), OBJECT_TYPE, "KenticoAlgolia.AlgoliaContentTypeItem", nameof(AlgoliaContentTypeItemId), null, nameof(AlgoliaContentTypeItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new(nameof(AlgoliaContentTypeItemIncludedPathItemId), AlgoliaIncludedPathItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
            new(nameof(AlgoliaContentTypeItemIndexItemId), AlgoliaIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Algolia content type item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaContentTypeItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaContentTypeItemId)), 0);
        set => SetValue(nameof(AlgoliaContentTypeItemId), value);
    }


    /// <summary>
    /// Algolia content type item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AlgoliaContentTypeItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AlgoliaContentTypeItemGuid)), default);
        set => SetValue(nameof(AlgoliaContentTypeItemGuid), value);
    }


    /// <summary>
    /// Content type name.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaContentTypeItemContentTypeName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaContentTypeItemContentTypeName)), String.Empty);
        set => SetValue(nameof(AlgoliaContentTypeItemContentTypeName), value);
    }


    /// <summary>
    /// Algolia included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaContentTypeItemIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaContentTypeItemIncludedPathItemId)), 0);
        set => SetValue(nameof(AlgoliaContentTypeItemIncludedPathItemId), value);
    }


    /// <summary>
    /// Algolia index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaContentTypeItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaContentTypeItemIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaContentTypeItemIndexItemId), value);
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
    /// Constructor for de-serialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected AlgoliaContentTypeItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaContentTypeItemInfo"/> class.
    /// </summary>
    public AlgoliaContentTypeItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaContentTypeItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaContentTypeItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
