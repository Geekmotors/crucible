using CrucibleAI.Tests.Constants;

namespace CrucibleAI.Tests;

public sealed class CrucibleTests
{
    [Fact]
    public async Task AGateWithInvalidOptionsIsAUsageErrorBeforeAnyRun()
    {
        var error = await Assert.ThrowsAsync<CrucibleException>(
            () => Crucible.Gate(new RunOptions { Runs = OptionValues.NoRuns }, _ => Task.CompletedTask, TestContext.Current.CancellationToken));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task AGateWithoutABodyIsAUsageError()
    {
        var error = await Assert.ThrowsAsync<CrucibleException>(() => Crucible.Gate(null!, TestContext.Current.CancellationToken));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task AGateWithoutOptionsIsAUsageError()
    {
        var error = await Assert.ThrowsAsync<CrucibleException>(() => Crucible.Gate(null!, _ => Task.CompletedTask, TestContext.Current.CancellationToken));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }
}
