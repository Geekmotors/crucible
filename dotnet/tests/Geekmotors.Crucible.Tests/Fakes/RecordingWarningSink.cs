using CrucibleAI.Providers;

namespace CrucibleAI.Tests.Fakes;

internal sealed class RecordingWarningSink : IWarningSink
{
    public List<string> Lines { get; } = [];

    public void Write(string warning) => Lines.Add(warning);
}
