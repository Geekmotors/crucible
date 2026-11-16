using CrucibleAI.Tracing;

namespace CrucibleAI.Tests.Fakes;

internal sealed class BrokenTraceOutput(Exception failure) : ITraceOutput
{
    public void Write(string text) => throw failure;
}
