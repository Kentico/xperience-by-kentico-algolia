using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Models.Common;

using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Contains methods to interface with the Algolia API.
    /// </summary>
    public interface IAlgoliaClient
    {
        /// <summary>
        /// Requests Algolia crawling of the specified <paramref name="urls"/>.
        /// </summary>
        /// <param name="crawlerId">The ID of the crawler to update.</param>
        /// <param name="urls">The URLs to crawl.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>The number of URLs crawled.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="FormatException" />
        /// <exception cref="OverflowException" />
        Task<int> CrawlUrls(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken);


        /// <summary>
        /// Removes records from the Algolia index.
        /// </summary>
        /// <param name="objectIds">The Algolia internal IDs of the records to delete.</param>
        /// <param name="indexName">The index containing the objects to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="OverflowException" />
        /// <returns>The number of records deleted.</returns>
        Task<int> DeleteRecords(IEnumerable<string> objectIds, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Deletes crawled URLs from the crawler's underlying index.
        /// </summary>
        /// <param name="crawlerId">The ID of the crawler to update.</param>
        /// <param name="urls">The URLs to delete from the index.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>The number of records deleted.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="FormatException" />
        /// <exception cref="OverflowException" />
        Task<int> DeleteUrls(string crawlerId, IEnumerable<string> urls, CancellationToken cancellationToken);


        /// <summary>
        /// Gets the full crawler details from Algolia's REST API.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/rest-api/crawler/#get-a-crawler"/>.</remarks>
        /// <param name="crawlerId">The ID of the crawler to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <returns>An <see cref="AlgoliaCrawler"/> with the <see cref="AlgoliaCrawler.Config"/> details, or
        /// <c>null</c> if there was an error retrieving the crawler.</returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="FormatException" />
        Task<AlgoliaCrawler> GetCrawler(string crawlerId, CancellationToken cancellationToken);


        /// <summary>
        /// Gets the indices of the Algolia application with basic statistics.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/api-reference/api-methods/list-indices/#response"/>.</remarks>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task<ICollection<IndicesResponse>> GetStatistics(CancellationToken cancellationToken);



        /// <summary>
        /// Updates the Algolia index with the dynamic data in each object of the passed <paramref name="dataObjects"/>.
        /// </summary>
        /// <remarks>Logs an error if there are issues loading the node data.</remarks>
        /// <param name="dataObjects">The objects to upsert into Algolia.</param>
        /// <param name="indexName">The index to upsert the data to.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        /// <exception cref="OverflowException" />
        /// <returns>The number of objects processed.</returns>
        Task<int> UpsertRecords(IEnumerable<JObject> dataObjects, string indexName, CancellationToken cancellationToken);


        /// <summary>
        /// Rebuilds the Algolia index by removing existing data from Algolia and indexing all
        /// pages in the content tree included in the index.
        /// </summary>
        /// <param name="indexName">The index to rebuild.</param>
        /// <param name="cancellationToken">The cancellation token for the task.</param>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="ObjectDisposedException" />
        Task Rebuild(string indexName, CancellationToken cancellationToken);
    }
}
