using CrucibleAI;
using CrucibleAI.Tests.Constants;

namespace Geekmotors.Consumer;

public sealed class ConsumerNamespaceTests
{
    [Fact]
    public async Task AGeekmotorsNamespaceReachesTheCrucibleEntryPoint()
    {
        var error = await Assert.ThrowsAsync<CrucibleException>(
            () => Crucible.Gate(new RunOptions { Runs = OptionValues.NoRuns }, _ => Task.CompletedTask, TestContext.Current.CancellationToken));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }
}
