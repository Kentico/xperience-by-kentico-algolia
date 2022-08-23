using Kentico.Xperience.AlgoliaSearch.Models;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Contains methods to interface with the Algolia API.
    /// </summary>
    public interface IAlgoliaClient
    {
        /// <summary>
        /// Removes records from the Algolia index.
        /// </summary>
        /// <param name="objectIds">The Algolia internal IDs of the records to delete.</param>
        /// <param name="indexName">The index containing the objects to delete.</param>
        /// <exception cref="ArgumentNullException" />
        /// <returns>The number of records deleted.</returns>
        Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName);


        /// <summary>
        /// Processes multiple queue items from all Algolia indexes in batches. Algolia
        /// automatically applies batching in multiples of 1,000 when using their API,
        /// so all queue items are forwarded to the API.
        /// </summary>
        /// <param name="items">The items to process.</param>
        /// <returns>The number of items processed.</returns>
        Task<int> ProcessAlgoliaTasks(IEnumerable<AlgoliaQueueItem> items);


        /// <summary>
        /// Updates the Algolia index with the dynamic data in each object of the passed <paramref name="dataObjects"/>.
        /// </summary>
        /// <remarks>Logs an error if there are issues loading the node data.</remarks>
        /// <param name="dataObjects">The objects to upsert into Algolia.</param>
        /// <param name="indexName">The index to upsert the data to.</param>
        /// <exception cref="ArgumentNullException" />
        /// <returns>The number of objects processed.</returns>
        Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName);


        /// <summary>
        /// Rebuilds the Algolia index by removing existing data from Algolia and indexing all
        /// pages in the content tree included in the index.
        /// </summary>
        /// <param name="indexName">The index to rebuild.</param>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ArgumentNullException" />
        Task Rebuild(string indexName);
    }
}
