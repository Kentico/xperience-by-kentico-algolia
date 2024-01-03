using Kentico.Xperience.Algolia.Services;
using System;
using System.Collections.Generic;

namespace Kentico.Xperience.Algolia;

public class StrategyStorage
{
    public static Dictionary<string, Type> Strategies { get; private set; }

    static StrategyStorage()
    {
        Strategies = new Dictionary<string, Type>();
    }

    public static void AddStrategy<TStrategy>(string strategyName) where TStrategy : IAlgoliaIndexingStrategy, new()
    {
        Strategies.Add(strategyName, typeof(TStrategy));
    }
}
