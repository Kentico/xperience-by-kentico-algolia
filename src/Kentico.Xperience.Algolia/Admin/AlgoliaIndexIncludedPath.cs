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
    public List<AlgoliaIndexContentType> ContentTypes { get; set; } = [];

    /// <summary>
    /// The internal identifier of the included path.
    /// </summary>
    public string? Identifier { get; set; }

    [JsonConstructor]
    public AlgoliaIndexIncludedPath(string aliasPath) => AliasPath = aliasPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaIndexIncludedPath"/> class from database entities,
    /// filtering content types to only those associated with this specific path.
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="contentTypesInfoItems"></param>
    /// <param name="contentTypeDisplayNames"></param>
    public AlgoliaIndexIncludedPath(AlgoliaIncludedPathItemInfo indexPath, IEnumerable<AlgoliaContentTypeItemInfo> contentTypesInfoItems, IReadOnlyDictionary<string, string> contentTypeDisplayNames)
    {
        AliasPath = indexPath.AlgoliaIncludedPathItemAliasPath;
        ContentTypes = contentTypesInfoItems
            .Where(ct => ct.AlgoliaContentTypeItemIncludedPathItemId == indexPath.AlgoliaIncludedPathItemId)
            .Select(ct => new AlgoliaIndexContentType(
                ct.AlgoliaContentTypeItemContentTypeName,
                contentTypeDisplayNames.GetValueOrDefault(ct.AlgoliaContentTypeItemContentTypeName, ct.AlgoliaContentTypeItemContentTypeName)))
            .ToList();
        Identifier = indexPath.AlgoliaIncludedPathItemId.ToString();
    }
}
