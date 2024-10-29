namespace Kentico.Xperience.Algolia.Search;

public class AlgoliaSearchResultModel
{
    public string Url { get; set; } = string.Empty;
    public string ContentTypeName { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public Guid ItemGuid { get; set; }
    public string ObjectID { get; set; } = string.Empty;
}
