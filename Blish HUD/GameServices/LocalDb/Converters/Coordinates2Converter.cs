using Gw2Sharp.Models;
using Newtonsoft.Json;
using System;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class Coordinates2Converter : JsonConverter<Coordinates2> {
        public override void WriteJson(JsonWriter writer, Coordinates2 value, JsonSerializer serializer)
            => serializer.Serialize(writer, new double[] { value.X, value.Y });

        public override Coordinates2 ReadJson(JsonReader reader, Type objectType, Coordinates2 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
            => serializer.Deserialize<double[]>(reader) is double[] { Length: 2 } coords
                ? new Coordinates2(coords[0], coords[1])
                : throw new JsonSerializationException();
    }
}
