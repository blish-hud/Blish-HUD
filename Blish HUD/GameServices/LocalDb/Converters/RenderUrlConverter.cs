using Gw2Sharp;
using Gw2Sharp.WebApi;
using Newtonsoft.Json;
using System;
using System.Reflection;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class RenderUrlConverter : JsonConverter<RenderUrl> {
        private readonly ConstructorInfo _constructorInfo;

        public RenderUrlConverter() {
            _constructorInfo = typeof(RenderUrl).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(IGw2Client), typeof(string), typeof(string) }, null) ??
                throw new JsonSerializationException($"No matching constructor found for type {typeof(RenderUrl)}");
        }

        public override void WriteJson(JsonWriter writer, RenderUrl value, JsonSerializer serializer)
            => serializer.Serialize(writer, value.Url.OriginalString);

        public override RenderUrl ReadJson(JsonReader reader, Type objectType, RenderUrl existingValue,
            bool hasExistingValue, JsonSerializer serializer) {
            string? url = serializer.Deserialize<string>(reader);
            return (RenderUrl)_constructorInfo.Invoke(new object?[] { null, url, null });
        }
    }
}
