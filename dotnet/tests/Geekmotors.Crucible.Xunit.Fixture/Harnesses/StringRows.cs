using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

public sealed class StringRows : TheoryData<string>
{
    public StringRows()
    {
        Add(FixtureTexts.Left);
        Add(FixtureTexts.Right);
    }
}
