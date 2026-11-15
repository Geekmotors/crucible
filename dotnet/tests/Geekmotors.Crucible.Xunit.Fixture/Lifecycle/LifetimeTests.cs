using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class LifetimeTests : IAsyncLifetime
{
    private CrucibleErrorKind? _initializeKind;
    private bool _bodyFinished;

    public ValueTask InitializeAsync()
    {
        _initializeKind = ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray));
        return ValueTask.CompletedTask;
    }

    [CrucibleFact(Runs = FixtureRuns.Few)]
    public async Task EachRunIsInitialisedOutsideTheRunAndDisposedAfterIt()
    {
        Assert.Equal(CrucibleErrorKind.Usage, _initializeKind);
        await FactProbe.AddAndCheckAsync(nameof(LifetimeTests));
        _bodyFinished = true;
    }

    public ValueTask DisposeAsync()
    {
        if (!_bodyFinished || ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray)) != CrucibleErrorKind.Usage)
        {
            throw new InvalidOperationException(FixtureTexts.Problem);
        }
        return ValueTask.CompletedTask;
    }
}
