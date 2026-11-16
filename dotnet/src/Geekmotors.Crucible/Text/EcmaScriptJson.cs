using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CrucibleAI.Text;

internal static class EcmaScriptJson
{
    private const int FirstPrintable = 0x20;

    public static string Value(object? value) => value switch
    {
        null => "null",
        bool flag => flag ? "true" : "false",
        string text => String(text),
        JsonNode node => Node(node),
        double number => Number(number),
        _ => throw new ArgumentException("Only null, booleans, numbers, strings and JSON nodes print as JSON.", nameof(value)),
    };

    public static string Node(JsonNode? node) => node switch
    {
        null => "null",
        JsonObject members => "{" + string.Join(",", members.Select(member => String(member.Key) + ":" + Node(member.Value))) + "}",
        JsonArray items => "[" + string.Join(",", items.Select(Node)) + "]",
        JsonValue value => Scalar(value),
        _ => throw new ArgumentException("Unknown JSON node.", nameof(node)),
    };

    public static string String(string text)
    {
        var json = new StringBuilder(text.Length + 2).Append('"');
        for (var index = 0; index < text.Length; index++)
        {
            var unit = text[index];
            if (char.IsHighSurrogate(unit) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
            {
                json.Append(unit).Append(text[++index]);
                continue;
            }
            json.Append(Escape(unit));
        }
        return json.Append('"').ToString();
    }

    private static string Scalar(JsonValue value) => value.GetValueKind() switch
    {
        JsonValueKind.String => String(value.GetValue<string>()),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Number => Number(double.Parse(value.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture)),
        _ => "null",
    };

    private static string Number(double value) => double.IsFinite(value) ? EcmaScriptNumber.Format(value) : "null";

    private static string Escape(char unit) => unit switch
    {
        '"' => "\\\"",
        '\\' => "\\\\",
        '\b' => "\\b",
        '\f' => "\\f",
        '\n' => "\\n",
        '\r' => "\\r",
        '\t' => "\\t",
        _ when unit < FirstPrintable || char.IsSurrogate(unit) => "\\u" + ((int)unit).ToString("x4", CultureInfo.InvariantCulture),
        _ => unit.ToString(),
    };
}
