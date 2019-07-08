using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api.Converters {
    public class RectangleConverter:JsonConverter {

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.StartArray) {
                var pointSets = JArray.Load(reader);

                var set1 = pointSets[0].Value<JArray>();
                var set2 = pointSets[1].Value<JArray>();

                return new Rectangle(set1[0].Value<int>(), set1[1].Value<int>(), set2[0].Value<int>(), set2[1].Value<int>());
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

    }
}
