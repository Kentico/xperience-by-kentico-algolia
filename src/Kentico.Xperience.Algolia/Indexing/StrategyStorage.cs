namespace Kentico.Xperience.Algolia.Indexing;

internal static class StrategyStorage
{
    public static Dictionary<string, Type> Strategies { get; private set; }
    static StrategyStorage() => Strategies = new Dictionary<string, Type>();

    public static void AddStrategy<TStrategy>(string strategyName) where TStrategy : IAlgoliaIndexingStrategy => Strategies.Add(strategyName, typeof(TStrategy));
}
