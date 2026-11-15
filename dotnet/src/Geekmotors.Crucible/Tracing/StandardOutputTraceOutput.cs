namespace CrucibleAI.Tracing;

internal sealed class StandardOutputTraceOutput : ITraceOutput
{
    public void Write(string text) => Console.Out.Write(text);
}
