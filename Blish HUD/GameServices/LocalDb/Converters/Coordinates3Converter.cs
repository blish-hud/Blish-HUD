using Gw2Sharp.Models;
using Newtonsoft.Json;
using System;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class Coordinates3Converter : JsonConverter<Coordinates3> {
        public override void WriteJson(JsonWriter writer, Coordinates3 value, JsonSerializer serializer)
            => serializer.Serialize(writer, new double[] { value.X, value.Y, value.Z });

        public override Coordinates3 ReadJson(JsonReader reader, Type objectType, Coordinates3 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
            => serializer.Deserialize<double[]>(reader) is double[] { Length: 3 } coords
                ? new Coordinates3(coords[0], coords[1], coords[2])
                : throw new JsonSerializationException();
    }
}
