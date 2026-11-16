using System.Text.Json.Nodes;
using CrucibleAI.Providers;

namespace CrucibleAI.Tests.Fakes;

internal sealed class EchoSystemJudgeClient : IJudgeClient
{
    public Task<string> CompleteAsync(JudgeRequest request, IJudgeObserver observer, CancellationToken cancellation) =>
        Task.FromResult(new JsonObject { ["verdict"] = true, ["value"] = null, ["reasoning"] = request.System }.ToJsonString());
}
