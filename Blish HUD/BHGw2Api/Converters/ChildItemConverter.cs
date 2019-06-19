using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.BHGw2Api.Converters {
    public class ChildItemConverter<T>:JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.StartObject) {
                var points = JObject.Load(reader);

                var dictPull = points.ToObject<Dictionary<String, T>>();

                return dictPull.Values.ToList();
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

    }
}
