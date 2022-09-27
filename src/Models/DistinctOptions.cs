using System;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Represents the distinct and de-duplication settings for the Algolia index.
    /// </summary>
    /// <remarks>See <see href="https://www.algolia.com/doc/guides/sending-and-managing-data/prepare-your-data/how-to/indexing-long-documents/"/>.</remarks>
    public sealed class DistinctOptions
    {
        /// <summary>
        /// The name of the attribute used for Algolia de-duplication.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/api-reference/api-parameters/attributeForDistinct"/>.</remarks>
        public string DistinctAttribute
        {
            get;
        }


        /// <summary>
        /// The distinction level.
        /// </summary>
        /// <remarks>See <see href="https://www.algolia.com/doc/api-reference/api-parameters/distinct"/>.</remarks>
        public int DistinctLevel
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctOptions"/> class.
        /// </summary>
        /// <param name="distinctAttribute">The name of the attribute used for Algolia de-duplication.</param>
        /// <param name="distinctLevel">The distinction level.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public DistinctOptions(string distinctAttribute, int distinctLevel)
        {
            if (String.IsNullOrEmpty(distinctAttribute))
            {
                throw new ArgumentNullException(nameof(distinctAttribute));
            }

            if (distinctLevel < 0)
            {
                throw new InvalidOperationException("Distinct level must be non-negative.");
            }

            DistinctAttribute = distinctAttribute;
            DistinctLevel = distinctLevel;
        }
    }
}