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
    /// Reconstructs an included path from persisted data, attaching only the content
    /// types that are linked to this specific path via
    /// <see cref="AlgoliaContentTypeItemInfo.AlgoliaContentTypeItemIncludedPathItemId"/>.
    /// </summary>
    /// <param name="indexPath">The persisted included path.</param>
    /// <param name="contentTypeItems">Content type link rows (may span multiple paths/indexes; filtered by path id).</param>
    /// <param name="contentTypes">Resolved content type metadata used to provide display names.</param>
    public AlgoliaIndexIncludedPath(
        AlgoliaIncludedPathItemInfo indexPath,
        IEnumerable<AlgoliaContentTypeItemInfo> contentTypeItems,
        IEnumerable<AlgoliaIndexContentType> contentTypes)
    {
        AliasPath = indexPath.AlgoliaIncludedPathItemAliasPath;

        var contentTypesByName = contentTypes
            .GroupBy(c => c.ContentTypeName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        ContentTypes = contentTypeItems
            .Where(ct => ct.AlgoliaContentTypeItemIncludedPathItemId == indexPath.AlgoliaIncludedPathItemId)
            .Select(ct => contentTypesByName.TryGetValue(ct.AlgoliaContentTypeItemContentTypeName, out var contentType)
                ? contentType
                : new AlgoliaIndexContentType(ct.AlgoliaContentTypeItemContentTypeName, ct.AlgoliaContentTypeItemContentTypeName))
            .ToList();
        Identifier = indexPath.AlgoliaIncludedPathItemId.ToString();
    }
}
