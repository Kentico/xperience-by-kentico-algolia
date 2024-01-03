using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS;

[assembly: RegisterObjectType(typeof(AlgoliaincludedpathitemInfo), AlgoliaincludedpathitemInfo.OBJECT_TYPE)]

namespace CMS
{
    /// <summary>
    /// Data container class for <see cref="AlgoliaincludedpathitemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AlgoliaincludedpathitemInfo : AbstractInfo<AlgoliaincludedpathitemInfo, IAlgoliaincludedpathitemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "algolia.algoliaincludedpathitem";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaincludedpathitemInfoProvider), OBJECT_TYPE, "algolia.algoliaincludedpathitem", "AlgoliaIncludedPathItemId", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("AlgoliaIndexItemId", "AlgoliaIndexitemInfo", ObjectDependencyEnum.Required),
            },
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
        public virtual string AliasPath
        {
            get => ValidationHelper.GetString(GetValue(nameof(AliasPath)), String.Empty);
            set => SetValue(nameof(AliasPath), value);
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
        protected AlgoliaincludedpathitemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AlgoliaincludedpathitemInfo"/> class.
        /// </summary>
        public AlgoliaincludedpathitemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AlgoliaincludedpathitemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AlgoliaincludedpathitemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}