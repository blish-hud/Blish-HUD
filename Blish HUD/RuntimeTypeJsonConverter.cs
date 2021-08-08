using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blish_HUD {

    internal class RuntimeTypeJsonConverter<T> : JsonConverter<T> {

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            (T)JsonSerializer.Deserialize(ref reader, typeToConvert, options);

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

}
