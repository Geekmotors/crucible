namespace CrucibleAI.Xunit.Fixture.Harnesses;

internal static class Signals
{
    public static TaskCompletionSource TimedOutRunCancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}
