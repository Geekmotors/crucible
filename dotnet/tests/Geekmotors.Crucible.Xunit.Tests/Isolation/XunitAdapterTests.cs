using CrucibleAI.Xunit.Tests.Constants;
using CrucibleAI.Xunit.Tests.Harnesses;
using Xunit.Sdk;

namespace CrucibleAI.Xunit.Tests.Isolation;

public sealed class XunitAdapterTests(FixtureRunHarness fixture) : IClassFixture<FixtureRunHarness>
{
    [Theory]
    [InlineData(FixtureTests.FirstInClass)]
    [InlineData(FixtureTests.SecondInClass)]
    public void TwoTestsInOneClassNeverSeeEachOthersFacts(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Theory]
    [InlineData(FixtureTests.ParallelLeft)]
    [InlineData(FixtureTests.ParallelRight)]
    public void TestsInClassesRunningInParallelNeverSeeEachOthersFacts(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Theory]
    [InlineData(FixtureTests.CollectionAlpha)]
    [InlineData(FixtureTests.CollectionBeta)]
    public void TestsInCollectionsRunningInParallelNeverSeeEachOthersFacts(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Theory]
    [InlineData(FixtureTests.InlineRows, FixtureTests.InlineRowCount)]
    [InlineData(FixtureTests.ClassDataRows, FixtureTests.DataRowCount)]
    [InlineData(FixtureTests.RunTimeRows, FixtureTests.DataRowCount)]
    public void EachTheoryRowIsOneResultWithItsOwnFacts(string test, int rows) =>
        Assert.Equal(Enumerable.Repeat(FixtureOutcome.Passed, rows), fixture.For(test).Select(result => result.Outcome));

    [Fact]
    public void FactsNeverSurviveFromOneTestToTheNext() =>
        Assert.All(
            new[] { FixtureTests.FirstInClass, FixtureTests.SecondInClass, FixtureTests.InlineRows },
            test => Assert.All(fixture.For(test), result => Assert.Equal(FixtureOutcome.Passed, result.Outcome)));

    [Fact]
    public void EachRunHasItsOwnFactsAndItsOwnClassInstance() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.RunInstances).Outcome);

    [Fact]
    public void EachRunIsInitialisedOutsideTheRunAndDisposedAfterItsBody() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.Lifetime).Outcome);

    [Fact]
    public void AnInstanceThatCannotBeDisposedFailsTheGate() =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(CrucibleVerdictException).FullName),
            (fixture.Single(FixtureTests.FailingDisposal).Outcome, fixture.Single(FixtureTests.FailingDisposal).ExceptionType));

    [Theory]
    [InlineData(FixtureTests.PlainFact)]
    [InlineData(FixtureTests.Constructor)]
    [InlineData(FixtureTests.StaticInitialiser)]
    [InlineData(FixtureTests.ClassFixture)]
    [InlineData(FixtureTests.CollectionFixture)]
    [InlineData(FixtureTests.Lifetime)]
    public void CrucibleCallsOutsideARunAreUsageErrors(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Fact]
    public void RunsExecuteAtOnceByDefault() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.Concurrent).Outcome);

    [Fact]
    public void SequentialRunsExecuteOneAtATime() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.Sequential).Outcome);

    [Theory]
    [InlineData(FixtureTests.ClassSetsRuns)]
    [InlineData(FixtureTests.MethodOverridesClass)]
    [InlineData(FixtureTests.AssemblySetsRuns)]
    public void ClassAndAssemblyAttributesSupplyDefaultsTheMethodOverrides(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Fact]
    public void AFailingGateFailsTheTestWithTheVerdictException() =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(CrucibleVerdictException).FullName),
            (fixture.Single(FixtureTests.FailingVerdict).Outcome, fixture.Single(FixtureTests.FailingVerdict).ExceptionType));

    [Fact]
    public void TheFailureMessageIsTheCrucibleReport() =>
        Assert.StartsWith(FixtureTests.FailedReportHeadline, fixture.Single(FixtureTests.FailingVerdict).Message, StringComparison.Ordinal);

    [Fact]
    public void ASkippedCrucibleTestNeverRuns() =>
        Assert.Equal(FixtureOutcome.Skipped, fixture.Single(FixtureTests.Skipped).Outcome);

    [Fact]
    public void XunitsTimeoutFailsTheTestAsTimedOut() =>
        Assert.Contains(typeof(TestTimeoutException).FullName, fixture.Single(FixtureTests.TimedOut).ExceptionTypes);

    [Fact]
    public void XunitsTestCancellationCancelsTheRuns() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.CancellationObserved).Outcome);

    [Fact]
    public void AStaticTestMethodRunsWithoutAnInstance() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.StaticMethod).Outcome);

    [Fact]
    public void AnInvalidAttributeSettingFailsTheTestAsAUsageError() =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(CrucibleException).FullName),
            (fixture.Single(FixtureTests.InvalidSettings).Outcome, fixture.Single(FixtureTests.InvalidSettings).ExceptionType));

    [Theory]
    [InlineData(FixtureTests.AsyncVoid)]
    [InlineData(FixtureTests.UnknownAwaitable)]
    public void AMethodXunitCannotAwaitIsRefused(string test) =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(TestPipelineException).FullName),
            (fixture.Single(test).Outcome, fixture.Single(test).ExceptionType));

    [Theory]
    [InlineData(FixtureTests.SynchronousShape)]
    [InlineData(FixtureTests.ValueTaskShape)]
    [InlineData(FixtureTests.ValueTaskOfTShape)]
    [InlineData(FixtureTests.TaskOfTShape)]
    public void SynchronousAndEveryAsyncReturnShapeRunsInsideItsRun(string test) =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(test).Outcome);

    [Theory]
    [InlineData(FixtureTests.FailingConstructor)]
    [InlineData(FixtureTests.FailingInitialise)]
    public void AnInstanceThatCannotBeBuiltFailsTheGate(string test) =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(CrucibleVerdictException).FullName),
            (fixture.Single(test).Outcome, fixture.Single(test).ExceptionType));

    [Theory]
    [InlineData(FixtureTests.AssertSkip)]
    [InlineData(FixtureTests.SkipException)]
    public void ARunThatSkipsSkipsTheTest(string test) =>
        Assert.Equal(FixtureOutcome.Skipped, fixture.Single(test).Outcome);

    [Fact]
    public void AUsageErrorARunThrowsFailsTheTestUnchanged() =>
        Assert.Equal(
            (FixtureOutcome.Failed, typeof(CrucibleException).FullName),
            (fixture.Single(FixtureTests.UsageError).Outcome, fixture.Single(FixtureTests.UsageError).ExceptionType));

    [Fact]
    public void EachRunInstanceReceivesTheOutputHelperAndTheAssemblyFixture() =>
        Assert.Equal(FixtureOutcome.Passed, fixture.Single(FixtureTests.Injection).Outcome);
}
