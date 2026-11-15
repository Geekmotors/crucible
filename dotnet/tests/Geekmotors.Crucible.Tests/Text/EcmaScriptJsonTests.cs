using System.Text.Json.Nodes;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Text;

namespace CrucibleAI.Tests.Text;

public sealed class EcmaScriptJsonTests
{
    private const char Quote = '"';
    private const char Backslash = '\\';

    [Fact]
    public void WritesNullTrueAndFalse() =>
        Assert.Equal(["null", "true", "false"], new object?[] { null, true, false }.Select(EcmaScriptJson.Value));

    [Fact]
    public void WritesNumbersAsNumberToString() =>
        Assert.Equal(["12.5", "-3", "0.9"], new object?[] { 12.5, -3d, 0.9 }.Select(EcmaScriptJson.Value));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void WritesANonFiniteNumberAsNull(double value) => Assert.Equal("null", EcmaScriptJson.Value(value));

    [Fact]
    public void WritesAParsedNumberBeyondDoubleRangeAsNull() =>
        Assert.Equal("[null]", EcmaScriptJson.Node(JsonNode.Parse("[1e999]")));

    [Fact]
    public void RejectsAValueThatIsNotAJsonScalar() =>
        Assert.Throws<ArgumentException>(() => EcmaScriptJson.Value(DayOfWeek.Monday));

    [Fact]
    public void EscapesOnlyQuoteBackslashAndShortControlEscapes() =>
        Assert.Equal(
            Quoted(Escaped(Quote) + Escaped(Backslash) + Escaped('b') + Escaped('f') + Escaped('n') + Escaped('r') + Escaped('t')),
            EcmaScriptJson.Value("\"\\\b\f\n\r\t"));

    [Fact]
    public void EscapesOtherControlsAsLowercaseUnicodeEscapes() =>
        Assert.Equal(Quoted(Backslash + "u001b" + Backslash + "u0000"), EcmaScriptJson.Value(UnicodeTexts.Escape + (char)0));

    [Fact]
    public void WritesNonAsciiAndAstralCharactersRaw()
    {
        var raw = UnicodeTexts.AccentedE + UnicodeTexts.Grinning + UnicodeTexts.LineSeparator + UnicodeTexts.Delete + "<>&'+";

        Assert.Equal(Quoted(raw), EcmaScriptJson.Value(raw));
    }

    [Fact]
    public void EscapesALoneSurrogateInLowercaseHex() =>
        Assert.Equal(
            Quoted(Backslash + "udc00" + Backslash + "ud800"),
            EcmaScriptJson.Value(UnicodeTexts.LoneLow + UnicodeTexts.LoneHigh));

    [Fact]
    public void WritesAnObjectCompactlyInMemberOrder() =>
        Assert.Equal(
            "{\"threshold\":0.8,\"runs\":3,\"strict\":true,\"name\":null}",
            EcmaScriptJson.Node(new JsonObject { ["threshold"] = 0.8, ["runs"] = 3, ["strict"] = true, ["name"] = null }));

    [Fact]
    public void WritesAnArrayCompactly() =>
        Assert.Equal("[1,\"a\",[]]", EcmaScriptJson.Node(new JsonArray(1, "a", new JsonArray())));

    private static string Quoted(string inner) => Quote + inner + Quote;

    private static string Escaped(char escape) => string.Concat(Backslash, escape);
}
