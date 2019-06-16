using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Version = SemVer.Version;

namespace Blish_HUD.Content.Serialization {
    public class SemVerConverter : JsonConverter<SemVer.Version> {

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        /// <inheritdoc />
        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer) {
             return new SemVer.Version((string)reader.Value, true);
        }

    }
}
