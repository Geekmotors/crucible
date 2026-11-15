using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Providers;

internal static class ChatRequestBody
{
    private const string ModelMember = "model";
    private const string MessagesMember = "messages";

    private static readonly JsonWriterOptions WriterOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static readonly JsonSerializerOptions NodeOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static byte[] Serialize(string model, JsonObject? meta, JudgeRequest request)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, WriterOptions))
        {
            writer.WriteStartObject();
            foreach (var (name, value) in meta ?? [])
            {
                if (name is ModelMember or MessagesMember)
                {
                    continue;
                }
                writer.WritePropertyName(name);
                if (value is null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    value.WriteTo(writer, NodeOptions);
                }
            }
            writer.WriteString(ModelMember, model);
            writer.WriteStartArray(MessagesMember);
            Message(writer, "system", request.System);
            Message(writer, "user", request.Prompt);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return buffer.ToArray();
    }

    private static void Message(Utf8JsonWriter writer, string role, string content)
    {
        writer.WriteStartObject();
        writer.WriteString("role", role);
        writer.WriteString("content", content);
        writer.WriteEndObject();
    }
}
