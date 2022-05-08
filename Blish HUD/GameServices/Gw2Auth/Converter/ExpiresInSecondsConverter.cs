using Newtonsoft.Json;
using System;

namespace Blish_HUD.GameServices.Gw2Auth.Converter {
    internal class ExpiresInSecondsConverter : JsonConverter<DateTime> {
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) {
            writer.WriteValue((long)value.Subtract(DateTime.UtcNow).TotalSeconds);
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if (reader.Value == null || reader.Value.GetType() != typeof(long)) {
                return DateTime.UtcNow;
            }
            return DateTime.UtcNow.AddSeconds((long)reader.Value);
        }
    }
}
