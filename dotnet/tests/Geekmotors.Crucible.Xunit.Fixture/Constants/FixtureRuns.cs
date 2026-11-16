namespace CrucibleAI.Xunit.Fixture.Constants;

internal static class FixtureRuns
{
    public const int AssemblyRuns = 2;
    public const int Few = 3;
    public const int ClassRuns = 4;
    public const int Several = 4;
    public const double Everyone = 1;
    public const int OneAtATime = 1;
    public const int MeetingTimeoutMs = 20000;
    public const int XunitTimeoutMs = 300;
    public const int LongTestTimeoutMs = 120000;
    public const int Single = 1;
    public const int Invalid = 0;
    public static readonly TimeSpan MeetingTimeout = TimeSpan.FromMilliseconds(MeetingTimeoutMs);
    public static readonly TimeSpan Pause = TimeSpan.FromMilliseconds(20);
}
