using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class ApiFlagsConverter : JsonConverter {
        public override bool CanConvert(Type objectType)
            => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ApiFlags<>);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            if (value == null) {
                serializer.Serialize(writer, null);
                return;
            }

            var type = value.GetType();
            var listProperty = type.GetProperty("List")
                ?? throw new JsonSerializationException($"No List property found for type {type}");

            object list = listProperty.GetValue(value);

            serializer.Serialize(writer, list);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer) {
            if (reader.TokenType != JsonToken.StartArray) {
                throw new JsonSerializationException("Cannot deserialize ApiFlags");
            }

            var enumType = objectType.GetGenericArguments()[0];
            var apiEnumType = typeof(ApiEnum<>).MakeGenericType(enumType);
            var listType = typeof(List<>).MakeGenericType(apiEnumType);

            object? rawValue = serializer.Deserialize(reader, listType);

            var constructorInfo = objectType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(IEnumerable<>).MakeGenericType(apiEnumType) },
                null);

            return constructorInfo.Invoke(new object?[] { rawValue });
        }
    }
}
