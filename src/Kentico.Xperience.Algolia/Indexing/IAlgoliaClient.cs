using Kentico.Xperience.Algolia.Admin;
using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Contains methods to interface with the Algolia API.
/// </summary>
public interface IAlgoliaClient
{
    /// <summary>
    /// Removes records from the 
    /// index.
    /// </summary>
    /// <param name="itemGuids">The Algolia internal IDs of the records to delete.</param>
    /// <param name="indexName">The index containing the objects to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// 
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of records deleted.</returns>
    Task<int> DeleteRecords(IEnumerable<string> itemGuids, string indexName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the indices of the Algolia application with basic statistics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task<ICollection<AlgoliaIndexStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken);

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

    /// <summary>
    /// Deletes the Algolia index by removing existing index data from Algolia.
    /// </summary>
    /// <param name="indexName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task DeleteIndex(string indexName, CancellationToken cancellationToken);
}
