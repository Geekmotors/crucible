using System.Runtime.CompilerServices;
using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Lifecycle;

public sealed class UnknownAwaitableTests
{
    [CrucibleFact(Runs = FixtureRuns.Single)]
    public YieldAwaitable IsRefused() => Task.Yield();
}
