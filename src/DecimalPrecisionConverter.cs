using Newtonsoft.Json;

using System;

namespace Kentico.Xperience.AlgoliaSearch
{
    /// <summary>
    /// Truncates decimal values to two places.
    /// </summary>
    public class DecimalPrecisionConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanRead
        {
            get { return false; }
        }


        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal));
        }


        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(FormattableString.Invariant($"{value:0.00}"));
        }


        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
