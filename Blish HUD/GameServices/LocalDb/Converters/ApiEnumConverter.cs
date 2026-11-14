using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Reflection;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class ApiEnumConverter : JsonConverter {
        public override bool CanConvert(Type objectType)
            => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ApiEnum<>);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            if (value == null) {
                serializer.Serialize(writer, null);
                return;
            }

            var type = value.GetType();
            var valueProperty = type.GetProperty("RawValue")
                ?? throw new JsonSerializationException($"No raw value property found for type {type}");

            string? rawValue = (string?)valueProperty.GetValue(value);

            serializer.Serialize(writer, rawValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer) {
            string? rawValue = serializer.Deserialize<string?>(reader);

            var constructorInfo = objectType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, new Type[] { typeof(string) }, null)
                    ?? throw new JsonSerializationException($"No constructor found for type {objectType}");

            return constructorInfo.Invoke(new object?[] { rawValue });
        }
    }
}
