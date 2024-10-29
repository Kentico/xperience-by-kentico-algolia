using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Algolia.Admin;

[assembly: RegisterObjectType(typeof(AlgoliaIndexItemInfo), AlgoliaIndexItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Data container class for <see cref="AlgoliaIndexItemInfo"/>.
/// </summary>
[Serializable]
public partial class AlgoliaIndexItemInfo : AbstractInfo<AlgoliaIndexItemInfo, IInfoProvider<AlgoliaIndexItemInfo>>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoalgolia.algoliaindexitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new(typeof(IInfoProvider<AlgoliaIndexItemInfo>), OBJECT_TYPE, "KenticoAlgolia.AlgoliaIndexItem", nameof(AlgoliaIndexItemId), null, nameof(AlgoliaIndexItemGuid), nameof(AlgoliaIndexItemIndexName), null, null, null, null)
    {
        TouchCacheDependencies = true,
        ContinuousIntegrationSettings =
        {
            Enabled = true,
        },
    };


    /// <summary>
    /// Algolia index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaIndexItemId), value);
    }


    /// <summary>
    /// Algolia index item Guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AlgoliaIndexItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AlgoliaIndexItemGuid)), default);
        set => SetValue(nameof(AlgoliaIndexItemGuid), value);
    }


    /// <summary>
    /// Index name.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexItemIndexName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexItemIndexName)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexItemIndexName), value);
    }


    /// <summary>
    /// Channel name.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexItemChannelName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexItemChannelName)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexItemChannelName), value);
    }


    /// <summary>
    /// Strategy name.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexItemStrategyName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexItemStrategyName)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexItemStrategyName), value);
    }


    /// <summary>
    /// Rebuild hook.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexItemRebuildHook
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexItemRebuildHook)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexItemRebuildHook), value, String.Empty);
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
    protected AlgoliaIndexItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaIndexItemInfo"/> class.
    /// </summary>
    public AlgoliaIndexItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaIndexItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaIndexItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
