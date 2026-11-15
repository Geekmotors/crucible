using System.Text.Json.Nodes;
using CrucibleAI.Assertions;
using CrucibleAI.Judging;
using CrucibleAI.Providers;
using CrucibleAI.Runs;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;

namespace CrucibleAI.Tests.Harnesses;

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

    public AssertionRun Run(bool strict = false) => new(Record, strict, JudgeTimes.InferenceTimeout, new QuietJudgeObserver(), CancellationToken.None);
}
