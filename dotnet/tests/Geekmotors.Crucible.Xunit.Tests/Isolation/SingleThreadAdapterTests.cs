using CrucibleAI.Xunit.Tests.Constants;
using CrucibleAI.Xunit.Tests.Harnesses;

namespace CrucibleAI.Xunit.Tests.Isolation;

public sealed class SingleThreadAdapterTests(SingleThreadFixtureRunHarness fixture) : IClassFixture<SingleThreadFixtureRunHarness>
{
    [Theory]
    [InlineData(FixtureTests.Concurrent)]
    [InlineData(FixtureTests.Sequential)]
    [InlineData(FixtureTests.Lifetime)]
    [InlineData(FixtureTests.StaticMethod)]
    [InlineData(FixtureTests.SynchronousShape)]
    public void UnderOneXunitThreadTheRunsStillRunAndNothingDeadlocks(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);
}
