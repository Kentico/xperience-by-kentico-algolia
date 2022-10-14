using System.Collections.Generic;

using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace Kentico.Xperience.Algolia.Tests
{
	/// <summary>
	/// Represents a content item of type Article.
	/// </summary>
	public partial class Article : TreeNode
	{
		#region "Constants and variables"

		/// <summary>
		/// The name of the data class.
		/// </summary>
		public const string CLASS_NAME = "Kentico.Xperience.Algolia.Tests.Article";


		/// <summary>
		/// The instance of the class that provides extended API for working with Article fields.
		/// </summary>
		private readonly ArticleFields mFields;

		#endregion


		#region "Properties"

		/// <summary>
		/// ArticleID.
		/// </summary>
		[DatabaseIDField]
		public int ArticleID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("ArticleID"), 0);
			}
			set
			{
				SetValue("ArticleID", value);
			}
		}


		/// <summary>
		/// ArticleTitle.
		/// </summary>
		[DatabaseField]
		public string ArticleTitle
		{
			get
			{
				return ValidationHelper.GetString(GetValue("ArticleTitle"), @"");
			}
			set
			{
				SetValue("ArticleTitle", value);
			}
		}


		/// <summary>
		/// ArticleTeaser.
		/// </summary>
		[DatabaseField]
		public string ArticleTeaser
		{
			get
			{
				return ValidationHelper.GetString(GetValue("ArticleTeaser"), "");
			}
			set
			{
				SetValue("ArticleTeaser", value);
			}
		}


		/// <summary>
		/// ArticleSummary.
		/// </summary>
		[DatabaseField]
		public string ArticleSummary
		{
			get
			{
				return ValidationHelper.GetString(GetValue("ArticleSummary"), @"");
			}
			set
			{
				SetValue("ArticleSummary", value);
			}
		}


		/// <summary>
		/// ArticleText.
		/// </summary>
		[DatabaseField]
		public string ArticleText
		{
			get
			{
				return ValidationHelper.GetString(GetValue("ArticleText"), @"");
			}
			set
			{
				SetValue("ArticleText", value);
			}
		}


		/// <summary>
		/// Gets an object that provides extended API for working with Article fields.
		/// </summary>
		[RegisterProperty]
		public ArticleFields Fields
		{
			get
			{
				return mFields;
			}
		}


		/// <summary>
		/// Provides extended API for working with Article fields.
		/// </summary>
		[RegisterAllProperties]
		public partial class ArticleFields : AbstractHierarchicalObject<ArticleFields>
		{
			/// <summary>
			/// The content item of type Article that is a target of the extended API.
			/// </summary>
			private readonly Article mInstance;


			/// <summary>
			/// Initializes a new instance of the <see cref="ArticleFields" /> class with the specified content item of type Article.
			/// </summary>
			/// <param name="instance">The content item of type Article that is a target of the extended API.</param>
			public ArticleFields(Article instance)
			{
				mInstance = instance;
			}


			/// <summary>
			/// ArticleID.
			/// </summary>
			public int ID
			{
				get
				{
					return mInstance.ArticleID;
				}
				set
				{
					mInstance.ArticleID = value;
				}
			}


			/// <summary>
			/// ArticleTitle.
			/// </summary>
			public string Title
			{
				get
				{
					return mInstance.ArticleTitle;
				}
				set
				{
					mInstance.ArticleTitle = value;
				}
			}


			/// <summary>
			/// ArticleTeaser.
			/// </summary>
			public IEnumerable<CMS.MediaLibrary.AssetRelatedItem> Teaser
			{
				get
				{
					return CMS.DataEngine.Internal.JsonDataTypeConverter.ConvertToModels<CMS.MediaLibrary.AssetRelatedItem>(mInstance.ArticleTeaser);
				}
			}


			/// <summary>
			/// ArticleSummary.
			/// </summary>
			public string Summary
			{
				get
				{
					return mInstance.ArticleSummary;
				}
				set
				{
					mInstance.ArticleSummary = value;
				}
			}


			/// <summary>
			/// ArticleText.
			/// </summary>
			public string Text
			{
				get
				{
					return mInstance.ArticleText;
				}
				set
				{
					mInstance.ArticleText = value;
				}
			}
		}

		#endregion


		#region "Constructors"

		/// <summary>
		/// Initializes a new instance of the <see cref="Article" /> class.
		/// </summary>
		public Article() : base(CLASS_NAME)
		{
			mFields = new ArticleFields(this);
		}

		#endregion
	}
}