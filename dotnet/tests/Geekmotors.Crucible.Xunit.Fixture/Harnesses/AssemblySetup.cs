using CrucibleAI;
using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

[assembly: AssemblyFixture(typeof(PromptJudgeInstaller))]
[assembly: Crucible(Runs = FixtureRuns.AssemblyRuns, Threshold = FixtureRuns.Everyone)]
