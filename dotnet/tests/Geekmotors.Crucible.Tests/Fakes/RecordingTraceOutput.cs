using System.Text;
using CrucibleAI.Tracing;

namespace CrucibleAI.Tests.Fakes;

internal sealed class RecordingTraceOutput : ITraceOutput
{
    private readonly StringBuilder _written = new();

    public string Text => _written.ToString();

    public void Write(string text) => _written.Append(text);
}
