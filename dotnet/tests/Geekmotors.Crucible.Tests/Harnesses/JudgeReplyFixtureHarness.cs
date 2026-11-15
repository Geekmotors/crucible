using System.Text.Json.Nodes;
using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class JudgeReplyFixtureHarness
{
    private static readonly Dictionary<string, ExtractType> Types = new(StringComparer.Ordinal)
    {
        ["string"] = ExtractType.String,
        ["number"] = ExtractType.Number,
        ["boolean"] = ExtractType.Boolean,
    };

    public static async Task<JsonNode?> AskAsync(JsonNode? input)
    {
        var replies = input!["replies"]!.AsArray().Select(reply => HttpAnswers.Reply(reply!.GetValue<string>())).ToArray();
        var client = new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(replies);
        var exchange = new JudgeExchange(client, new ManualTimeProvider());
        if (input["ask"]!.GetValue<string>() == "coherent")
        {
            var coherent = await exchange.AskAsync(
                ProviderHarness.Request, new CoherentReplyReader().Read, JudgeTimes.InferenceTimeout, CancellationToken.None);
            return new JsonObject { ["verdict"] = coherent.Verdict, ["reasoning"] = coherent.Reasoning };
        }
        var reader = new ExtractReplyReader(Types[input["type"]!.GetValue<string>()]);
        var extract = await exchange.AskAsync(ProviderHarness.Request, reader.Read, JudgeTimes.InferenceTimeout, CancellationToken.None);
        return new JsonObject { ["value"] = Value(extract.Value), ["reasoning"] = extract.Reasoning };
    }

    private static JsonValue? Value(object? value) => value switch
    {
        string text => JsonValue.Create(text),
        double number => JsonValue.Create(number),
        bool flag => JsonValue.Create(flag),
        _ => null,
    };
}
