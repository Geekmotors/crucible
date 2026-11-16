using System.Text.Json.Nodes;
using CrucibleAI.Providers;

namespace CrucibleAI.Tests.Fakes;

internal sealed class EchoJudgeClient(JsonNode? value = null, Task? release = null) : IJudgeClient
{
    public async Task<string> CompleteAsync(JudgeRequest request, IJudgeObserver observer, CancellationToken cancellation)
    {
        await (release ?? Task.CompletedTask);
        return new JsonObject { ["verdict"] = true, ["value"] = value?.DeepClone(), ["reasoning"] = request.Prompt }.ToJsonString();
    }
}
