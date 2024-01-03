using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS;

[assembly: RegisterObjectType(typeof(AlgoliaindexitemInfo), AlgoliaindexitemInfo.OBJECT_TYPE)]

namespace CMS
{
    /// <summary>
    /// Data container class for <see cref="AlgoliaindexitemInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AlgoliaindexitemInfo : AbstractInfo<AlgoliaindexitemInfo, IAlgoliaindexitemInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "algolia.algoliaindexitem";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AlgoliaindexitemInfoProvider), OBJECT_TYPE, "algolia.algoliaindexitem", "AlgoliaIndexItemId", null, null, "IndexName", null, null, null, null)
        {
            TouchCacheDependencies = true,
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
        /// Index name.
        /// </summary>
        [DatabaseField]
        public virtual string IndexName
        {
            get => ValidationHelper.GetString(GetValue(nameof(IndexName)), String.Empty);
            set => SetValue(nameof(IndexName), value);
        }


        /// <summary>
        /// Channel name.
        /// </summary>
        [DatabaseField]
        public virtual string ChannelName
        {
            get => ValidationHelper.GetString(GetValue(nameof(ChannelName)), String.Empty);
            set => SetValue(nameof(ChannelName), value);
        }


        /// <summary>
        /// Strategy name.
        /// </summary>
        [DatabaseField]
        public virtual string StrategyName
        {
            get => ValidationHelper.GetString(GetValue(nameof(StrategyName)), String.Empty);
            set => SetValue(nameof(StrategyName), value);
        }


        /// <summary>
        /// Rebuild hook.
        /// </summary>
        [DatabaseField]
        public virtual string RebuildHook
        {
            get => ValidationHelper.GetString(GetValue(nameof(RebuildHook)), String.Empty);
            set => SetValue(nameof(RebuildHook), value, String.Empty);
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
        protected AlgoliaindexitemInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AlgoliaindexitemInfo"/> class.
        /// </summary>
        public AlgoliaindexitemInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AlgoliaindexitemInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AlgoliaindexitemInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}