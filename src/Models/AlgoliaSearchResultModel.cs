namespace Kentico.Xperience.Algolia.Models;

public class AlgoliaSearchResultModel
{
    /// <summary>
    /// The absolute live site URL of the indexed page.
    /// </summary>
    public string Url { get; set; }

    public string ClassName { get; set; }

    public string LanguageCode { get; set; }

    public string ObjectID { get; set; }
}
