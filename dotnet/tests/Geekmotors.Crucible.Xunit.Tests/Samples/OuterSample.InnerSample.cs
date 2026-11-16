using CrucibleAI.Xunit.Tests.Constants;

namespace CrucibleAI.Xunit.Tests.Samples;

internal sealed partial class OuterSample
{
    [Crucible(Runs = SampleValues.InnerRuns)]
    internal sealed class InnerSample;
}
