using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class RectangleConverter : JsonConverter<Rectangle> {
        private class Intermediate {
            public Coordinates2 TopLeft;
            public Coordinates2 TopRight;
            public Coordinates2 BottomLeft;
            public Coordinates2 BottomRight;
            public RectangleDirectionType Direction;
        }

        public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer)
            => serializer.Serialize(writer, new Intermediate() {
                TopLeft = value.TopLeft,
                TopRight = value.TopRight,
                BottomLeft = value.BottomLeft,
                BottomRight = value.BottomRight,
                Direction = value.Direction,
            });

        public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue,
            bool hasExistingValue, JsonSerializer serializer)
            => serializer.Deserialize<Intermediate>(reader) is Intermediate i
                ? new Rectangle(
                    i.Direction == RectangleDirectionType.BottomUp ? i.BottomLeft : i.TopLeft,
                    i.Direction == RectangleDirectionType.BottomUp ? i.TopRight : i.BottomRight,
                    i.Direction)
                : throw new JsonSerializationException();
    }
}
