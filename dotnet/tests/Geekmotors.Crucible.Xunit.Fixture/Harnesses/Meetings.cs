using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

internal static class Meetings
{
    public static Rendezvous ParallelClasses { get; } = new(FixtureRuns.Few * 2);

    public static Rendezvous Collections { get; } = new(FixtureRuns.Few * 2);
}
