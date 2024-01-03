using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS;

[assembly: RegisterObjectType(typeof(AlgoliacontenttypeitemInfo), AlgoliacontenttypeitemInfo.OBJECT_TYPE)]

namespace CMS
{
    /// <summary>
    /// Data container class for <see cref="AlgoliacontenttypeitemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AlgoliacontenttypeitemInfo : AbstractInfo<AlgoliacontenttypeitemInfo, IAlgoliacontenttypeitemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "algolia.algoliacontenttypeitem";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliacontenttypeitemInfoProvider), OBJECT_TYPE, "algolia.algoliacontenttypeitem", "AlgoliaContentTypeItemId", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("AlgoliaIncludedPathItemId", "AlgoliaIncludedpathitemInfo", ObjectDependencyEnum.Required),
                new ObjectDependency("AlgoliaIndexItemId", "AlgoliaIndexitemInfo", ObjectDependencyEnum.Required),
            },
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
        /// Content type name.
        /// </summary>
        [DatabaseField]
        public virtual string ContentTypeName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ContentTypeName)), String.Empty);
            set => SetValue(nameof(ContentTypeName), value);
        }


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
        protected AlgoliacontenttypeitemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AlgoliacontenttypeitemInfo"/> class.
        /// </summary>
        public AlgoliacontenttypeitemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AlgoliacontenttypeitemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AlgoliacontenttypeitemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}