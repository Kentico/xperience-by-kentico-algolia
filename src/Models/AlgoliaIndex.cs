using System;

namespace Kentico.Xperience.Algolia.Models
{
    /// <summary>
    /// Represents the configuration of an Algolia index.
    /// </summary>
    public sealed class AlgoliaIndex
    {
        /// <summary>
        /// The type of the class which extends <see cref="AlgoliaSearchModel"/>.
        /// </summary>
        public Type Type
        {
            get;
        }


        /// <summary>
        /// The code name of the Algolia index.
        /// </summary>
        public string IndexName
        {
            get;
        }


        /// <summary>
        /// An arbitrary ID used to identify the Algolia index in the admin UI.
        /// </summary>
        internal int Identifier
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new <see cref="AlgoliaIndex"/>.
        /// </summary>
        /// <param name="type">The type of the class which extends <see cref="AlgoliaSearchModel"/>.</param>
        /// <param name="indexName">The code name of the Algolia index.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        public AlgoliaIndex(Type type, string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!typeof(AlgoliaSearchModel).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"The search model {type} must extend {nameof(AlgoliaSearchModel)}.");
            }

            Type = type;
            IndexName = indexName;
        }
    }
}
