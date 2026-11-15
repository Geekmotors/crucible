using CrucibleAI.Xunit.Fixture.Fakes;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

public sealed class PromptJudgeInstaller
{
    public PromptJudgeInstaller() => Crucible.UseJudge(new PromptJudgeClient());
}
