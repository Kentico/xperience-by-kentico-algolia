using System.ComponentModel.DataAnnotations;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Algolia.Admin.Providers;

namespace Kentico.Xperience.Algolia.Admin;

public class AlgoliaConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(
       Label = "Index Name",
       ExplanationText = "Changing this value on an existing index without changing application code will cause the search experience to stop working.",
       Order = 1)]
    [Required]
    [MinLength(1)]
    public string IndexName { get; set; } = string.Empty;

    [AlgoliaIndexConfigurationComponent(Label = "Included Paths", Order = 2)]
    public IEnumerable<AlgoliaIndexIncludedPath> Paths { get; set; } = new List<AlgoliaIndexIncludedPath>();

    [GeneralSelectorComponent(dataProviderType: typeof(ReusableContentOptionsProvider), Label = "Included Reusable Content Types", Order = 3)]
    public IEnumerable<string> ReusableContentTypeNames { get; set; } = Enumerable.Empty<string>();

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Indexed Languages", Order = 4)]
    public IEnumerable<string> LanguageNames { get; set; } = Enumerable.Empty<string>();

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 5)]
    public string ChannelName { get; set; } = string.Empty;

    [DropDownComponent(Label = "Indexing Strategy", DataProviderType = typeof(IndexingStrategyOptionsProvider), Order = 6)]
    public string StrategyName { get; set; } = string.Empty;

    [TextInputComponent(Label = "Rebuild Hook", Order = 7)]
    public string RebuildHook { get; set; } = string.Empty;

    public AlgoliaConfigurationModel() { }

    public AlgoliaConfigurationModel(
        AlgoliaIndexItemInfo index,
        IEnumerable<AlgoliaIndexLanguageItemInfo> indexLanguages,
        IEnumerable<AlgoliaIncludedPathItemInfo> indexPaths,
        IEnumerable<AlgoliaIndexContentType> contentTypes,
        IEnumerable<AlgoliaReusableContentTypeItemInfo> reusableContentTypes
    )
    {
        Id = index.AlgoliaIndexItemId;
        IndexName = index.AlgoliaIndexItemIndexName;
        ChannelName = index.AlgoliaIndexItemChannelName;
        RebuildHook = index.AlgoliaIndexItemRebuildHook;
        StrategyName = index.AlgoliaIndexItemStrategyName;
        LanguageNames = indexLanguages
            .Where(l => l.AlgoliaIndexLanguageItemIndexItemId == index.AlgoliaIndexItemId)
            .Select(l => l.AlgoliaIndexLanguageItemName)
            .ToList();
        ReusableContentTypeNames = reusableContentTypes
              .Where(c => c.AlgoliaReusableContentTypeItemIndexItemId == index.AlgoliaIndexItemId)
              .Select(c => c.AlgoliaReusableContentTypeItemContentTypeName)
              .ToList();
        Paths = indexPaths
            .Where(p => p.AlgoliaIncludedPathItemIndexItemId == index.AlgoliaIndexItemId)
            .Select(p => new AlgoliaIndexIncludedPath(p, contentTypes))
            .ToList();
    }
}
