namespace Kentico.Xperience.Algolia.Admin;

public class AlgoliaIndexContentType
{
    /// <summary>
    /// Name of the indexed content type for an indexed path
    /// </summary>
    public string ContentTypeName { get; set; } = "";

    /// <summary>
    /// Displayed name of the indexed content type for an indexed path which will be shown in admin UI
    /// </summary>
    public string ContentTypeDisplayName { get; set; } = "";

    public AlgoliaIndexContentType()
    { }

    public AlgoliaIndexContentType(string className, string classDisplayName)
    {
        ContentTypeName = className;
        ContentTypeDisplayName = classDisplayName;
    }
}
