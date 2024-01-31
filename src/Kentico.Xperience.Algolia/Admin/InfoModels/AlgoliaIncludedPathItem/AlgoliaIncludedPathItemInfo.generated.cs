using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Algolia.Admin;

[assembly: RegisterObjectType(typeof(AlgoliaIncludedPathItemInfo), AlgoliaIncludedPathItemInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Admin;

/// <summary>
/// Data container class for <see cref="AlgoliaIncludedPathItemInfo"/>.
/// </summary>
[Serializable]
public partial class AlgoliaIncludedPathItemInfo : AbstractInfo<AlgoliaIncludedPathItemInfo, IAlgoliaIncludedPathItemInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "kenticoalgolia.algoliaincludedpathitem";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaIncludedPathItemInfoProvider), OBJECT_TYPE, "KenticoAlgolia.AlgoliaIncludedPathItem", nameof(AlgoliaIncludedPathItemId), null, nameof(AlgoliaIncludedPathItemGuid), null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new (nameof(AlgoliaIncludedPathIndexItemId), AlgoliaIndexItemInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
        },
        ContinuousIntegrationSettings =
        {
            Enabled = true
        }
    };


    /// <summary>
    /// Algolia included path item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIncludedPathItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIncludedPathItemId)), 0);
        set => SetValue(nameof(AlgoliaIncludedPathItemId), value);
    }


    /// <summary>
    /// Alias path.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIncludedPathAliasPath
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIncludedPathAliasPath)), String.Empty);
        set => SetValue(nameof(AlgoliaIncludedPathAliasPath), value);
    }


    /// <summary>
    /// Algolia index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIncludedPathIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIncludedPathIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaIncludedPathIndexItemId), value);
    }


    /// <summary>
    /// Algolia included path item guid.
    /// </summary>
    [DatabaseField]
    public virtual Guid AlgoliaIncludedPathItemGuid
    {
        get => ValidationHelper.GetGuid(GetValue(nameof(AlgoliaIncludedPathItemGuid)), default);
        set => SetValue(nameof(AlgoliaIncludedPathItemGuid), value);
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
    protected AlgoliaIncludedPathItemInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaIncludedPathItemInfo"/> class.
    /// </summary>
    public AlgoliaIncludedPathItemInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaIncludedPathItemInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaIncludedPathItemInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
