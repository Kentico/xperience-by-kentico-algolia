namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Represents the type of an <see cref="AlgoliaQueueItem"/>.
/// </summary>
public enum AlgoliaTaskType
{
    /// <summary>
    /// Unsupported task type.
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// A task for a page which was published for the first time.
    /// </summary>
    PUBLISH_INDEX,

    /// <summary>
    /// A task for a page which was previously published.
    /// </summary>
    UPDATE,

    /// <summary>
    /// A task for a page which should be removed from the index.
    /// </summary>
    DELETE
}
