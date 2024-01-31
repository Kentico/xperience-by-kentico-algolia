using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Algolia.Admin.Providers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
    public string IndexName { get; set; } = "";

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Indexed Languages", Order = 2)]
    public IEnumerable<string> LanguageNames { get; set; } = Enumerable.Empty<string>();

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 3)]
    public string ChannelName { get; set; } = "";

    [DropDownComponent(Label = "Indexing Strategy", DataProviderType = typeof(IndexingStrategyOptionsProvider), Order = 4)]
    public string StrategyName { get; set; } = "";

    [TextInputComponent(Label = "Rebuild Hook")]
    public string RebuildHook { get; set; } = "";

    [AlgoliaIndexConfigurationComponent(Label = "Included Paths")]
    public IEnumerable<AlgoliaIndexIncludedPath> Paths { get; set; } = new List<AlgoliaIndexIncludedPath>();

    public AlgoliaConfigurationModel() { }

    public AlgoliaConfigurationModel(
        AlgoliaIndexItemInfo index,
        IEnumerable<AlgoliaIndexLanguageItemInfo> indexLanguages,
        IEnumerable<AlgoliaIncludedPathItemInfo> indexPaths,
        IEnumerable<AlgoliaContentTypeItemInfo> contentTypes
    )
    {
        Id = index.AlgoliaIndexItemId;
        IndexName = index.AlgoliaIndexItemIndexName;
        ChannelName = index.AlgoliaIndexItemChannelName;
        RebuildHook = index.AlgoliaIndexItemRebuildHook;
        StrategyName = index.AlgoliaIndexItemStrategyName;
        LanguageNames = indexLanguages.Select(l => l.AlgoliaIndexLanguageItemName).ToList();
        Paths = indexPaths.Select(p => new AlgoliaIndexIncludedPath(p, contentTypes)).ToList();
    }
}
