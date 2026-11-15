namespace CrucibleAI.Xunit.Fixture.Constants;

internal static class FixtureTexts
{
    public const string TokenFormat = "[fact of {0} / run {1}]";
    public const string TokenPattern = @"\[fact of [^\]]+ / run \d+\]";
    public const string Response = "The meeting is at six.";
    public const string Target = "the facts the judge received";
    public const string Problem = "the system under test misbehaved";
    public const string Skipped = "skipped on purpose";
    public const string Stray = "a fact written outside any run";
    public const string Left = "left";
    public const string Right = "right";
    public const string One = "one";
    public const string Two = "two";
    public const string Three = "three";
}
