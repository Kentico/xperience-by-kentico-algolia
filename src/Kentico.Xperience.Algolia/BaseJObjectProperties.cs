using Newtonsoft.Json.Linq;
using Kentico.Xperience.Algolia.Indexing;

namespace Kentico.Xperience.Algolia;

/// <summary>
/// Properties automatically added to each indexed item
/// </summary>
public static class BaseJObjectProperties
{
    public const string OBJECT_ID = "objectID";
    public const string CONTENT_TYPE_NAME = "ContentTypeName";
    public const string ITEM_GUID = "ItemGuid";
    public const string LANGUAGE_NAME = "LanguageName";
    /// <summary>
    /// By default this field on the <see cref="JObject"/> is populated with a web page's relative path
    /// if the indexed item is a web page. The field is not added to a JObject for reusable content items.
    /// 
    /// If a field with this name has already been added to the JObject by
    /// custom <see cref="IAlgoliaIndexingStrategy.MapToAlgoliaJObjectsOrNull"/> it will not be overridden.
    /// This enables a developer to choose if they want to use relative or absolute URLs
    /// </summary>
    public const string URL = "Url";
}
