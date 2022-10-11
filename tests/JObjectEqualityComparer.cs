using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Algolia.Tests
{
    /// <summary>
    /// Compares the <see cref="JObject.Properties"/> of two objects to ensure they
    /// contain the same properties and property values.
    /// </summary>
    internal class JObjectEqualityComparer : IEqualityComparer<JObject>
    {
        /// <inheritdoc/>
        public bool Equals(JObject x, JObject y)
        {
            if (y.Properties().Count() != x.Properties().Count())
            {
                return false;
            }

            foreach (var property in x)
            {
                var matchingProperty = y.Property(property.Key);
                if (matchingProperty == null)
                {
                    return false;
                }

                if (!property.Value.Equals(matchingProperty.Value))
                {
                    return false;
                }
            }

            return true;
        }


        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] JObject obj)
        {
            return obj.GetHashCode();
        }
    }
}
