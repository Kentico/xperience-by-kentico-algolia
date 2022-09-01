using Kentico.Xperience.Algolia.Models;

namespace Kentico.Xperience.Algolia
{
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
        CREATE,

        /// <summary>
        /// A task for a page which was previously published.
        /// </summary>
        UPDATE,

        /// <summary>
        /// A task for a page which should be removed from the index.
        /// </summary>
        DELETE
    }
}
