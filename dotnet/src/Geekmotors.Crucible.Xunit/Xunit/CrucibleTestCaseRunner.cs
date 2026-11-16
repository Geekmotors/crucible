using Xunit.Sdk;
using Xunit.v3;

namespace CrucibleAI.Xunit;

internal sealed class CrucibleTestCaseRunner : XunitTestCaseRunnerBase<CrucibleTestCaseRunnerContext, IXunitTestCase, IXunitTest>
{
    public static CrucibleTestCaseRunner Instance { get; } = new();

    public async ValueTask<RunSummary> Run(
        IXunitTestCase testCase,
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        var tests = await aggregator.RunAsync(testCase.CreateTests, []).ConfigureAwait(false);
        if (aggregator.ToException() is { } failure)
        {
            return failure.Message?.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal) == true
                ? XunitRunnerHelper.SkipTestCases(messageBus, cancellationTokenSource, [testCase], failure.Message[DynamicSkipToken.Value.Length..])
                : XunitRunnerHelper.FailTestCases(messageBus, cancellationTokenSource, [testCase], failure);
        }
        var context = new CrucibleTestCaseRunnerContext(
            testCase,
            tests,
            explicitOption,
            messageBus,
            aggregator,
            testCase.TestCaseDisplayName,
            testCase.SkipReason,
            cancellationTokenSource,
            parallelMode,
            scheduler,
            constructorArguments,
            methodFixtureMappings);
        await using (context.ConfigureAwait(false))
        {
            await context.InitializeAsync().ConfigureAwait(false);
            return await Run(context).ConfigureAwait(false);
        }
    }
}
