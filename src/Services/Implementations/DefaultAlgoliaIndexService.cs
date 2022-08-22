using Algolia.Search.Clients;

using CMS;
using CMS.Core;

using Kentico.Xperience.AlgoliaSearch.Services;

[assembly: RegisterImplementation(typeof(IAlgoliaIndexService), typeof(DefaultAlgoliaIndexService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexService"/>.
    /// </summary>
    internal class DefaultAlgoliaIndexService : IAlgoliaIndexService
    {
        private readonly ISearchClient searchClient;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAlgoliaIndexService"/> class.
        /// </summary>
        public DefaultAlgoliaIndexService(ISearchClient searchClient)
        {
            this.searchClient = searchClient;
        }


        public ISearchIndex InitializeIndex(string indexName)
        {
            return searchClient.InitIndex(indexName);
        }
    }
}