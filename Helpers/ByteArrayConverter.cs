using System.Text.Json;
using System.Text.Json.Serialization;

public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<byte>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Number && reader.TryGetByte(out byte b))
                    list.Add(b);
                else
                    throw new JsonException("Expected byte value in array.");
            }
            return list.ToArray();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? base64 = reader.GetString();
            if (base64 == null) throw new JsonException("String value was null.");
            return Convert.FromBase64String(base64);
        }

        throw new JsonException("Unsupported format for byte array.");
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var b in value)
            writer.WriteNumberValue(b);
        writer.WriteEndArray();
    }
}
