using System;

namespace Kentico.Xperience.Algolia.Search;

public class AlgoliaSearchResultModel
{
    public string Url { get; set; }
    public string ContentTypeName { get; set; }
    public string LanguageName { get; set; }
    public Guid ItemGuid { get; set; }
    public string ObjectID { get; set; }
}
