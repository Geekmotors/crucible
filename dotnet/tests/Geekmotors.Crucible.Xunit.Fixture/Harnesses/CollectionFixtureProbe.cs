using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

public sealed class CollectionFixtureProbe
{
    public CrucibleErrorKind? Kind { get; } = ErrorKindProbe.Of(() => Crucible.Facts(FixtureTexts.Stray));
}
