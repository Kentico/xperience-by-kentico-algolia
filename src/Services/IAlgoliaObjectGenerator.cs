using System;
using System.Collections.Generic;

using CMS.DocumentEngine;

using Kentico.Xperience.Algolia.Models;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Services
{
    /// <summary>
    /// Generates anonymous <see cref="JObject"/>s from Xperience <see cref="TreeNode"/>s
    /// for upserting into Algolia.
    /// </summary>
    public interface IAlgoliaObjectGenerator
    {
        /// <summary>
        /// Creates an anonymous object with the indexed column names of the <paramref name="searchModelType"/> and
        /// their values loaded from the passed <paramref name="node"/>.
        /// </summary>
        /// <remarks>When the <paramref name="taskType"/> is <see cref="AlgoliaTaskType.UPDATE"/>, only the updated
        /// columns will be included in the resulting object for a partial update. For <see cref="AlgoliaTaskType.CREATE"/>,
        /// all indexed columns are included.</remarks>
        /// <param name="node">The <see cref="TreeNode"/> to load values from.</param>
        /// <param name="searchModelType">The class of the Algolia search model.</param>
        /// <param name="taskType">The Algolia task being processed.</param>
        /// <returns>The anonymous data that will be passed to Algolia.</returns>
        /// <exception cref="ArgumentNullException" />
        JObject GetTreeNodeData(TreeNode node, Type searchModelType, AlgoliaTaskType taskType);


        /// <summary>
        /// Splits a <see cref="TreeNode"/>s data into multiple fragments. The "objectID" of each individual fragment
        /// <u>must be set</u> within this method.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/guides/sending-and-managing-data/prepare-your-data/how-to/indexing-long-documents/"/>.</remarks>
        /// <param name="originalData">The data from the <see cref="TreeNode"/>.</param>
        /// <param name="algoliaIndex">The index which the <see cref="TreeNode"/> belongs to.</param>
        /// <returns>One or more anonymous objects that will be passed to Algolia.</returns>
        IEnumerable<JObject> SplitData(JObject originalData, AlgoliaIndex algoliaIndex);
    }
}
