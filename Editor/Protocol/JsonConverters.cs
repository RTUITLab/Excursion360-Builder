using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.Protocol
{
    public class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        public static readonly QuaternionJsonConverter Instance = new();
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.WritePropertyName("x");
            writer.WriteValue(value.normalized.x);

            writer.WritePropertyName("y");
            writer.WriteValue(value.y);

            writer.WritePropertyName("z");
            writer.WriteValue(value.z);

            writer.WritePropertyName("w");
            writer.WriteValue(value.w);

            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Used only for writing");
        }
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        public static readonly ColorJsonConverter Instance = new();
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("r");
            writer.WriteValue(value.r);

            writer.WritePropertyName("g");
            writer.WriteValue(value.g);

            writer.WritePropertyName("b");
            writer.WriteValue(value.b);

            writer.WritePropertyName("a");
            writer.WriteValue(value.a);

            writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Used only for writing");
        }
    }
}
