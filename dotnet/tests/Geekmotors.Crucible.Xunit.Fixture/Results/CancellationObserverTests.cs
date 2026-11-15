using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Results;

public sealed class CancellationObserverTests
{
    [Fact]
    public Task TheTimedOutRunWasCancelled() =>
        Signals.TimedOutRunCancelled.Task.WaitAsync(FixtureRuns.MeetingTimeout, TestContext.Current.CancellationToken);
}
