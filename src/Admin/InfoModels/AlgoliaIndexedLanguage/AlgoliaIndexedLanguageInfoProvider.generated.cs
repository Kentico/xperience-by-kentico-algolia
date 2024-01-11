using CMS.DataEngine;

namespace Kentico.Xperience.Algolia.Models;

/// <summary>
/// Class providing <see cref="AlgoliaIndexedLanguageInfo"/> management.
/// </summary>
[ProviderInterface(typeof(IAlgoliaIndexedLanguageInfoProvider))]
public partial class AlgoliaIndexedLanguageInfoProvider : AbstractInfoProvider<AlgoliaIndexedLanguageInfo, AlgoliaIndexedLanguageInfoProvider>, IAlgoliaIndexedLanguageInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlgoliaIndexedLanguageInfoProvider"/> class.
    /// </summary>
    public AlgoliaIndexedLanguageInfoProvider()
        : base(AlgoliaIndexedLanguageInfo.TYPEINFO)
    {
    }
}