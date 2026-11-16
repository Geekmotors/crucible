using Xunit.Sdk;
using Xunit.v3;

namespace CrucibleAI.Xunit;

internal sealed class CrucibleTestCaseRunnerContext(
    IXunitTestCase testCase,
    IReadOnlyCollection<IXunitTest> tests,
    ExplicitOption explicitOption,
    IMessageBus messageBus,
    ExceptionAggregator aggregator,
    string displayName,
    string? skipReason,
    CancellationTokenSource cancellationTokenSource,
    ParallelMode parallelMode,
    ExecutionScheduler scheduler,
    object?[] constructorArguments,
    FixtureMappingManager methodFixtureMappings)
    : XunitTestCaseRunnerBaseContext<IXunitTestCase, IXunitTest>(
        testCase,
        tests,
        explicitOption,
        messageBus,
        aggregator,
        displayName,
        skipReason,
        cancellationTokenSource,
        parallelMode,
        scheduler,
        constructorArguments,
        methodFixtureMappings)
{
    public override ValueTask<RunSummary> RunTest(IXunitTest test) =>
        CrucibleTestRunner.Instance.Run(
            test,
            MessageBus,
            ConstructorArguments,
            ExplicitOption,
            Aggregator.Clone(),
            CancellationTokenSource,
            ParallelMode,
            Scheduler,
            BeforeAfterTestAttributes,
            CaseFixtureMappings);
}
