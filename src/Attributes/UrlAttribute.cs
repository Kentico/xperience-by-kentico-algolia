using System;

namespace Kentico.Xperience.AlgoliaSearch.Attributes
{
    /// <summary>
    /// A property attribute which specifies that the value of the property
    /// should be converted to an absolute URL during indexing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class UrlAttribute : Attribute
    {
    }
}