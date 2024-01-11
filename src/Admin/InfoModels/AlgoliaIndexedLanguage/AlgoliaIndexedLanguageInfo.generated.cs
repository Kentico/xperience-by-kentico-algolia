using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Kentico.Xperience.Algolia.Models;

[assembly: RegisterObjectType(typeof(AlgoliaIndexedLanguageInfo), AlgoliaIndexedLanguageInfo.OBJECT_TYPE)]

namespace Kentico.Xperience.Algolia.Models;

/// <summary>
/// Data container class for <see cref="AlgoliaIndexedLanguageInfo"/>.
/// </summary>
[Serializable]
public partial class AlgoliaIndexedLanguageInfo : AbstractInfo<AlgoliaIndexedLanguageInfo, IAlgoliaIndexedLanguageInfoProvider>
{
    /// <summary>
    /// Object type.
    /// </summary>
    public const string OBJECT_TYPE = "algolia.algoliaindexedlanguage";


    /// <summary>
    /// Type information.
    /// </summary>
    public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaIndexedLanguageInfoProvider), OBJECT_TYPE, "algolia.algoliaindexedlanguage", "IndexedLanguageId", null, null, null, null, null, null, null)
    {
        TouchCacheDependencies = true,
        DependsOn = new List<ObjectDependency>()
        {
            new ObjectDependency("AlgoliaIndexItemId", "AlgoliaIndexItemInfo", ObjectDependencyEnum.Required),
        },
    };


    /// <summary>
    /// Indexed language id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIndexedLanguageId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIndexedLanguageId)), 0);
        set => SetValue(nameof(AlgoliaIndexedLanguageId), value);
    }


    /// <summary>
    /// Code.
    /// </summary>
    [DatabaseField]
    public virtual string AlgoliaIndexedLanguageName
    {
        get => ValidationHelper.GetString(GetValue(nameof(AlgoliaIndexedLanguageName)), String.Empty);
        set => SetValue(nameof(AlgoliaIndexedLanguageName), value);
    }


    /// <summary>
    /// Algolia index item id.
    /// </summary>
    [DatabaseField]
    public virtual int AlgoliaIndexedLanguageIndexItemId
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(AlgoliaIndexedLanguageIndexItemId)), 0);
        set => SetValue(nameof(AlgoliaIndexedLanguageIndexItemId), value);
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
    protected AlgoliaIndexedLanguageInfo(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }


    /// <summary>
    /// Creates an empty instance of the <see cref="AlgoliaIndexedLanguageInfo"/> class.
    /// </summary>
    public AlgoliaIndexedLanguageInfo()
        : base(TYPEINFO)
    {
    }


    /// <summary>
    /// Creates a new instances of the <see cref="AlgoliaIndexedLanguageInfo"/> class from the given <see cref="DataRow"/>.
    /// </summary>
    /// <param name="dr">DataRow with the object data.</param>
    public AlgoliaIndexedLanguageInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}