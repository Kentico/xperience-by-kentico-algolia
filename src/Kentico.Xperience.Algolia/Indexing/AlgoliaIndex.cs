using Kentico.Xperience.Algolia.Admin;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Represents the configuration of an Algolia index.
/// </summary>
public sealed class AlgoliaIndex
{
    /// <summary>
    /// An arbitrary ID used to identify the Algolia index in the admin UI.
    /// </summary>
    public int Identifier { get; set; }

    /// <summary>
    /// The code name of the Algolia index.
    /// </summary>
    public string IndexName { get; }

    /// <summary>
    /// The Name of the WebSiteChannel.
    /// </summary>
    public string WebSiteChannelName { get; }

    /// <summary>
    /// The Language used on the WebSite on the Channel which is indexed.
    /// </summary>
    public List<string> LanguageNames { get; }

    /// <summary>
    /// A list of reusable content types that will be indexed.
    /// </summary>
    public List<string> IncludedReusableContentTypes { get; }

    /// <summary>
    /// The type of the class which extends <see cref="IAlgoliaIndexingStrategy"/>.
    /// </summary>
    public Type AlgoliaIndexingStrategyType { get; }

    internal IEnumerable<AlgoliaIndexIncludedPath> IncludedPaths { get; set; }

    internal AlgoliaIndex(AlgoliaConfigurationModel indexConfiguration, Dictionary<string, Type> strategies)
    {
        Identifier = indexConfiguration.Id;
        IndexName = indexConfiguration.IndexName;
        WebSiteChannelName = indexConfiguration.ChannelName;
        LanguageNames = indexConfiguration.LanguageNames.ToList();
        IncludedPaths = indexConfiguration.Paths;
        IncludedReusableContentTypes = indexConfiguration.ReusableContentTypeNames.ToList();

        var strategy = typeof(DefaultAlgoliaIndexingStrategy);

        if (strategies.ContainsKey(indexConfiguration.StrategyName))
        {
            strategy = strategies[indexConfiguration.StrategyName];
        }

        AlgoliaIndexingStrategyType = strategy;
    }
}
