namespace CrucibleAI.Tests.Fakes;

internal sealed class StalledSynchronizationContext : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? state)
    {
    }

    public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException();

    public override SynchronizationContext CreateCopy() => this;
}
