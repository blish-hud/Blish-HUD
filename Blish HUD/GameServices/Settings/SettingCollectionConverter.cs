using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blish_HUD.Settings {

    internal class SettingCollectionConverter : JsonConverter<ISettingCollection> {

        public override ISettingCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<SettingCollection>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, ISettingCollection value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

}
