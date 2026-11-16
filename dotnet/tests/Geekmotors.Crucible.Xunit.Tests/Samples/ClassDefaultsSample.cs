using CrucibleAI.Xunit.Tests.Constants;

namespace CrucibleAI.Xunit.Tests.Samples;

[Crucible(Runs = SampleValues.ClassRuns, Threshold = SampleValues.ClassThreshold, Sequential = true)]
internal sealed class ClassDefaultsSample;
