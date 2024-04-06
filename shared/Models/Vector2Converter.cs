using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace shared.Models;

public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (JsonDocument.TryParseValue(ref reader, out var doc))
        {
            var x = doc.RootElement.GetProperty("X").GetDouble();
            var y = doc.RootElement.GetProperty("Y").GetDouble();
            return new Vector2((float)x, (float)y);
        }
        throw new JsonException("Unable to parse Vector2.");
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}