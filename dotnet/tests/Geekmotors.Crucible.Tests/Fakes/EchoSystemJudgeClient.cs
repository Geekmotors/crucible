using System.Text.Json.Nodes;
using Geekmotors.Crucible.Providers;

namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class EchoSystemJudgeClient : IJudgeClient
{
    public Task<string> CompleteAsync(JudgeRequest request, CancellationToken cancellation) =>
        Task.FromResult(new JsonObject { ["verdict"] = true, ["value"] = null, ["reasoning"] = request.System }.ToJsonString());
}
