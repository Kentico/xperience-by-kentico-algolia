using Kentico.Xperience.AlgoliaSearch.Models;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Contains a collection of <see cref="AlgoliaIndex"/> which are automatically registered by
    /// <see cref="IAlgoliaRegistrationService"/> during application startup.
    /// </summary>
    public interface IAlgoliaIndexStore
    {
        /// <summary>
        /// Inserts an <see cref="AlgoliaIndex"/> into the register.
        /// </summary>
        /// <typeparam name="TModel">The search model class.</typeparam>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <returns>The <see cref="IAlgoliaRegistrationService"/> for chaining.</returns>
        IAlgoliaIndexStore Add<TModel>(string indexName) where TModel : AlgoliaSearchModel;


        /// <summary>
        /// Pops off the first <see cref="AlgoliaIndex"/> in the register, or <c>null</c> if the register
        /// is empty.
        /// </summary>
        AlgoliaIndex Pop();
    }
}
