using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }

    public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        float x = jsonObject.GetValue("x").Value<float>();
        float y = jsonObject.GetValue("y").Value<float>();
        float z = jsonObject.GetValue("z").Value<float>();
        return new Vector3(x, y, z);
    }
}