using CrucibleAI.Tests.Constants;
using CrucibleAI.Text;

namespace CrucibleAI.Tests.Text;

public sealed class TextFormatTests
{
    private const string Continuation = "    ";

    [Fact]
    public void CutKeepsTheFirstUnits() => Assert.Equal("abc", TextFormat.Cut("abcdef", 3));

    [Fact]
    public void CutKeepsATextWithinTheLimit() => Assert.Equal("ab", TextFormat.Cut("ab", 3));

    [Fact]
    public void CutDropsAHighSurrogateItWouldSplit() =>
        Assert.Equal("ab", TextFormat.Cut("ab" + UnicodeTexts.Grinning, 3));

    [Fact]
    public void CutAtOneKeepsAWholePair() =>
        Assert.Equal(UnicodeTexts.Grinning, TextFormat.Cut(UnicodeTexts.Grinning + "x", 1));

    [Fact]
    public void IndentedPrefixesLaterLines() =>
        Assert.Equal("a\n    b\n    c", TextFormat.Indented("a\nb\nc", Continuation));

    [Fact]
    public void IndentedSplitsAtEveryReportLineBreak() =>
        Assert.Equal(
            "a\n    b\n    c\n    d\n    e",
            TextFormat.Indented("a\r\nb\rc" + UnicodeTexts.LineSeparator + "d" + UnicodeTexts.ParagraphSeparator + "e", Continuation));

    [Fact]
    public void IndentedLeavesOtherBreakLikeCharactersInTheLine()
    {
        var line = "a" + UnicodeTexts.NextLine + "b" + UnicodeTexts.VerticalTab + "c" + UnicodeTexts.FormFeed + "d";

        Assert.Equal(line, TextFormat.Indented(line, Continuation));
    }

    [Fact]
    public void IndentedLeavesBlankLinesUnprefixed() =>
        Assert.Equal("a\n\n" + UnicodeTexts.NoBreakSpace + "\n    b", TextFormat.Indented("a\n\n" + UnicodeTexts.NoBreakSpace + "\nb", Continuation));
}
