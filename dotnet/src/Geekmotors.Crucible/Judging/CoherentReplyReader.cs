using System.Text.Json.Nodes;

namespace CrucibleAI.Judging;

internal sealed class CoherentReplyReader
{
    public CoherentReply? Read(JsonNode? reply) =>
        reply is JsonObject members
        && members["verdict"] is JsonValue verdict && verdict.TryGetValue<bool>(out var verdictValue)
        && members["reasoning"] is JsonValue reasoning && reasoning.TryGetValue<string>(out var reasoningText)
            ? new CoherentReply(verdictValue, reasoningText)
            : null;
}
