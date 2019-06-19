using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Blish_HUD.BHGw2Api.Converters {
    public class PointConverter:JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.StartArray) {
                var points = JArray.Load(reader);

                if (points.Count == 2) {
                    return new Point(points[0].Value<int>(), points[1].Value<int>());
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

    }
}
