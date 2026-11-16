using System.Text.Json.Nodes;

namespace Geekmotors.Crucible.Judging;

internal sealed class ExtractReplyReader(ExtractType type)
{
    public ExtractReply? Read(JsonNode? reply)
    {
        if (reply is not JsonObject members
            || !members.TryGetPropertyValue("value", out var value)
            || members["reasoning"] is not JsonValue reasoning
            || !reasoning.TryGetValue<string>(out var reasoningText))
        {
            return null;
        }
        if (value is null)
        {
            return new ExtractReply(null, reasoningText);
        }
        return Typed(value) is { } typed ? new ExtractReply(typed, reasoningText) : null;
    }

    private object? Typed(JsonNode value) => (type, value) switch
    {
        (ExtractType.String, JsonValue text) when text.TryGetValue<string>(out var stringValue) => stringValue,
        (ExtractType.Number, JsonValue number) when number.TryGetValue<double>(out var numberValue) && double.IsFinite(numberValue) => numberValue,
        (ExtractType.Boolean, JsonValue flag) when flag.TryGetValue<bool>(out var flagValue) => flagValue,
        _ => null,
    };
}
