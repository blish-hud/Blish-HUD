using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Blish_HUD.BHGw2Api.Converters {
    public class ColorConverter:JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.StartArray) {
                var clrComponents = JArray.Load(reader);

                if (clrComponents.Count == 3)
                    return new Gw2Color(clrComponents[0].Value<int>(), clrComponents[1].Value<int>(), clrComponents[2].Value<int>());
                else
                    Console.WriteLine("Colors API provided malformed color."); // TODO: More than this for exception handling
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

    }
}
