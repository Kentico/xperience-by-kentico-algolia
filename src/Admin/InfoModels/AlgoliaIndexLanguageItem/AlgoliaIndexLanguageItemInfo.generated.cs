using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Algolia.Models;

[assembly: RegisterObjectType(typeof(AlgoliaIndexLanguageItemInfo), AlgoliaIndexLanguageItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Models;

/// <summary>
/// Data container class for <see cref="AlgoliaIndexLanguageItemInfo"/>.
/// </summary>
[Serializable]
public partial class AlgoliaIndexLanguageItemInfo : AbstractInfo<AlgoliaIndexLanguageItemInfo, IAlgoliaIndexLanguageItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoalgolia.algoliaindexedlanguage";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaIndexLanguageInfoProvider), OBJECT_TYPE, "KenticoAlgolia.AlgoliaIndexLanguage", nameof(AlgoliaIndexLanguageItemId), null, nameof(AlgoliaIndexLanguageItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new (nameof(AlgoliaIndexLanguageItemIndexItemId), AlgoliaIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Indexed language id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIndexLanguageItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIndexLanguageItemId)), 0);
        set => SetValue(nameof(AlgoliaIndexLanguageItemId), value);
    }


    /// <summary>
    /// Indexed language id.
    /// </summary>
    [DatabaseField]
    public virtual Guid AlgoliaIndexLanguageItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AlgoliaIndexLanguageItemGuid)), default);
        set => SetValue(nameof(AlgoliaIndexLanguageItemGuid), value);
    }


    /// <summary>
    /// Code.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexedLanguageItemName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexedLanguageItemName)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexedLanguageItemName), value);
    }


    /// <summary>
    /// Algolia index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIndexLanguageItemIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIndexLanguageItemIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaIndexLanguageItemIndexItemId), value);
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
    protected AlgoliaIndexLanguageItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaIndexLanguageItemInfo"/> class.
    /// </summary>
    public AlgoliaIndexLanguageItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaIndexLanguageItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaIndexLanguageItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}