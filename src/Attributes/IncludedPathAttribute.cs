using System;

namespace Kentico.Xperience.Algolia.Attributes
{
    /// <summary>
    /// A class attribute applied to an Algolia search model indicating that the specified path, page
    /// type(s), and cultures are included in the index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IncludedPathAttribute : Attribute
    {
        /// <summary>
        /// The node alias pattern that will be used to match pages in the content tree for indexing.
        /// </summary>
        /// <remarks>For example, "/Blogs/Products/%" will index all pages under the "Products" page.</remarks>
        public string AliasPath
        {
            get;
        }


        /// <summary>
        /// A list of page types under the specified <see cref="AliasPath"/> that will be indexed.
        /// If empty, all page types are indexed.
        /// </summary>
        public string[] PageTypes
        {
            get;
            set;
        } = Array.Empty<string>();


        /// <summary>
        /// The internal identifier of the included path.
        /// </summary>
        internal string Identifier
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="IncludedPathAttribute"/> class.
        /// </summary>
        /// <param name="aliasPath">The node alias pattern that will be used to match pages in the content tree
        /// for indexing.</param>
        public IncludedPathAttribute(string aliasPath)
        {
            AliasPath = aliasPath;
        }
    }
}