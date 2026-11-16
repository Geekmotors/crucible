using Geekmotors.Crucible.Tracing;

namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class BrokenTraceOutput(Exception failure) : ITraceOutput
{
    public void Write(string text) => throw failure;
}
