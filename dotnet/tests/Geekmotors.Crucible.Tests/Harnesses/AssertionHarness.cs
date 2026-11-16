using System.Text.Json.Nodes;
using Geekmotors.Crucible.Assertions;
using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Providers;
using Geekmotors.Crucible.Runs;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed class AssertionHarness(IJudgeClient client)
{
    private readonly JudgeExchange _exchange = new(client, new ManualTimeProvider());

    public AssertionHarness(JsonNode? value = null, Task? release = null)
        : this(new EchoJudgeClient(value, release))
    {
    }

    public RunRecord Record { get; } = new();

    public CoherentAssertion Coherent => new(_exchange);

    public ExtractAssertion Extract => new(_exchange);

    public AssertionRun Run(bool strict = false) => new(Record, strict, JudgeTimes.InferenceTimeout, CancellationToken.None);
}
