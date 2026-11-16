using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class InvalidSettingsTests
{
    [CrucibleFact(Runs = FixtureRuns.Invalid)]
    public Task FailsAsAUsageError() => Task.CompletedTask;
}
