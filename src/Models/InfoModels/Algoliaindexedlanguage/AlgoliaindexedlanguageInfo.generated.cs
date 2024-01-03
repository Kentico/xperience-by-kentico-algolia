using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS;

[assembly: RegisterObjectType(typeof(AlgoliaindexedlanguageInfo), AlgoliaindexedlanguageInfo.OBJECT_TYPE)]

namespace CMS
{
    /// <summary>
    /// Data container class for <see cref="AlgoliaindexedlanguageInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AlgoliaindexedlanguageInfo : AbstractInfo<AlgoliaindexedlanguageInfo, IAlgoliaindexedlanguageInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "algolia.algoliaindexedlanguage";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaindexedlanguageInfoProvider), OBJECT_TYPE, "algolia.algoliaindexedlanguage", "IndexedLanguageId", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("AlgoliaIndexItemId", "AlgoliaIndexitemInfo", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Indexed language id.
        /// </summary>
        [DatabaseField]
        public virtual int IndexedLanguageId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(IndexedLanguageId)), 0);
            set => SetValue(nameof(IndexedLanguageId), value);
        }


        /// <summary>
        /// Code.
        /// </summary>
        [DatabaseField]
        public virtual string languageCode
        {
            get => ValidationHelper.GetString(GetValue(nameof(languageCode)), String.Empty);
            set => SetValue(nameof(languageCode), value);
        }


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
        protected AlgoliaindexedlanguageInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AlgoliaindexedlanguageInfo"/> class.
        /// </summary>
        public AlgoliaindexedlanguageInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AlgoliaindexedlanguageInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AlgoliaindexedlanguageInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}