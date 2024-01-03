using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Algolia.Admin.Components;
using Kentico.Xperience.Algolia.Admin.Providers;
using System.Collections.Generic;

namespace Kentico.Xperience.Algolia.Models;

public class AlgoliaConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(Label = "Index Name", Order = 1)]
    public string? IndexName { get; set; }

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Indexed Languages", Order = 2)]
    public IEnumerable<string>? LanguageNames { get; set; }

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 3)]
    public string? ChannelName { get; set; }

    [DropDownComponent(Label = "Indexing Strategy", DataProviderType = typeof(IndexingStrategyOptionsProvider), Order = 4)]
    public string? StrategyName { get; set; }

    [TextInputComponent(Label = "Rebuild Hook")]
    public string? RebuildHook { get; set; }

    [PathComponent(Label = "Included Paths")]
    public List<IncludedPath>? Paths { get; set; }
}
