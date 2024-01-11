using Kentico.Xperience.Algolia.Indexing;
using System;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Returns an instance of the <see cref="IAlgoliaIndexingStrategy"/> assigned to the given <paramref name="index" />.
    /// Used to generate instances of a <see cref="IAlgoliaIndexingStrategy"/> service type that can change at runtime.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="index"></param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the assigned <see cref="IAlgoliaIndexingStrategy"/> cannot be instantiated.
    ///     This shouldn't normally occur because we fallback to <see cref="DefaultAlgoliaIndexingStrategy" /> if not custom strategy is specified.
    ///     However, incorrect dependency management in user-code could cause issues.
    /// </exception>
    /// <returns></returns>
    public static IAlgoliaIndexingStrategy GetRequiredStrategy(this IServiceProvider serviceProvider, AlgoliaIndex index)
    {
        var strategy = serviceProvider.GetRequiredService(index.AlgoliaIndexingStrategyType) as IAlgoliaIndexingStrategy;

        return strategy!;
    }
}
