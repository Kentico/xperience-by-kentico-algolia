using Kentico.Xperience.Algolia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Kentico.Xperience.Algolia.Admin;

public class AlgoliaIndexIncludedPath
{
    /// <summary>
    /// The node alias pattern that will be used to match pages in the content tree for indexing.
    /// </summary>
    /// <remarks>For example, "/Blogs/Products/" will index all pages under the "Products" page.</remarks>
    public string AliasPath { get; }

    /// <summary>
    /// A list of content types under the specified <see cref="AliasPath"/> that will be indexed.
    /// </summary>
    public List<string> ContentTypes { get; set; } = new();

    /// <summary>
    /// The internal identifier of the included path.
    /// </summary>
    public string? Identifier { get; set; }

    [JsonConstructor]
    public AlgoliaIndexIncludedPath(string aliasPath) => AliasPath = aliasPath;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="contentTypes"></param>
    public AlgoliaIndexIncludedPath(AlgoliaIncludedPathItemInfo indexPath, IEnumerable<AlgoliaContentTypeItemInfo> contentTypes)
    {
        AliasPath = indexPath.AlgoliaIncludedPathAliasPath;
        ContentTypes = contentTypes.Where(y => indexPath.AlgoliaIncludedPathItemId == y.AlgoliaContentTypeItemIncludedPathItemId).Select(y => y.AlgoliaContentTypeItemContentTypeName).ToList();
        Identifier = indexPath.AlgoliaIncludedPathItemId.ToString();
    }
}
