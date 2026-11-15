using Xunit.Sdk;
using Xunit.v3;

namespace CrucibleAI.Xunit;

internal sealed class CrucibleDelayEnumeratedTestCase : XunitDelayEnumeratedTheoryTestCase, ISelfExecutingXunitTestCase
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public CrucibleDelayEnumeratedTestCase()
    {
    }

    public CrucibleDelayEnumeratedTestCase(
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueID,
        bool @explicit,
        bool skipTestWithoutData,
        Type[]? skipExceptions,
        string? skipReason,
        Type? skipType,
        string? skipUnless,
        string? skipWhen,
        Dictionary<string, HashSet<string>>? traits,
        string? sourceFilePath,
        int? sourceLineNumber,
        int? timeout)
        : base(
            testMethod,
            testCaseDisplayName,
            uniqueID,
            @explicit,
            skipTestWithoutData,
            skipExceptions,
            skipReason,
            skipType,
            skipUnless,
            skipWhen,
            traits,
            sourceFilePath,
            sourceLineNumber,
            timeout)
    {
    }

    public ValueTask<RunSummary> Run(
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings) =>
        CrucibleTestCaseRunner.Instance.Run(
            this, explicitOption, messageBus, constructorArguments, aggregator, cancellationTokenSource, parallelMode, scheduler, methodFixtureMappings);
}
