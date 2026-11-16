namespace CrucibleAI.Tests.Constants;

internal static class BackoffTimes
{
    public const int FirstRetry = 1;
    public const int SecondRetry = 2;
    public const double NoJitter = 0;
    public const double FullJitter = 1;
    public static readonly TimeSpan FirstRetryWithoutJitter = TimeSpan.FromMilliseconds(250);
    public static readonly TimeSpan SecondRetryWithoutJitter = TimeSpan.FromMilliseconds(500);
    public static readonly TimeSpan SecondRetryWithFullJitter = TimeSpan.FromMilliseconds(1000);
    public static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(1);
}
