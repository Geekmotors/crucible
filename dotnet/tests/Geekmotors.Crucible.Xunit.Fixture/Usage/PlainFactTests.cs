using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Usage;

public sealed class PlainFactTests
{
    [Fact]
    public void APlainFactCannotAddFacts() =>
        Assert.Equal(CrucibleErrorKind.Usage, ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray)));
}
