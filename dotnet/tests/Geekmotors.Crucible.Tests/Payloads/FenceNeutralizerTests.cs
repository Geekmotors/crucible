using Geekmotors.Crucible.Payloads;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Payloads;

public sealed class FenceNeutralizerTests
{
    private const string Close = ">>>";
    private const string Open = "<<<";

    public static TheoryData<string, string> Neutralized => new()
    {
        { Close, " >>>" },
        { " >>>", "  >>>" },
        { "  >>>  ", "   >>>  " },
        { "\t<<<", " \t<<<" },
        { "a\r\n>>>\r\nb", "a\r\n >>>\r\nb" },
        { "a\r>>>\rb", "a\r >>>\rb" },
        { "a" + UnicodeTexts.LineSeparator + Close + UnicodeTexts.ParagraphSeparator + "b", "a" + UnicodeTexts.LineSeparator + " " + Close + UnicodeTexts.ParagraphSeparator + "b" },
        { "a" + UnicodeTexts.VerticalTab + Close + UnicodeTexts.FormFeed + "b", "a" + UnicodeTexts.VerticalTab + " " + Close + UnicodeTexts.FormFeed + "b" },
        { "a" + UnicodeTexts.NextLine + Open, "a" + UnicodeTexts.NextLine + " " + Open },
        { UnicodeTexts.ZeroWidthSpace + Close, " " + UnicodeTexts.ZeroWidthSpace + Close },
        { ">" + UnicodeTexts.ZeroWidthSpace + ">>", " >" + UnicodeTexts.ZeroWidthSpace + ">>" },
        { UnicodeTexts.TagLetterA + Close, " " + UnicodeTexts.TagLetterA + Close },
        { Close + UnicodeTexts.MusicalSymbolBeginBeam, " " + Close + UnicodeTexts.MusicalSymbolBeginBeam },
        { UnicodeTexts.NoBreakSpace + Open + UnicodeTexts.IdeographicSpace + UnicodeTexts.ByteOrderMark, " " + UnicodeTexts.NoBreakSpace + Open + UnicodeTexts.IdeographicSpace + UnicodeTexts.ByteOrderMark },
        { "x\n>>>\n>>>\ny", "x\n >>>\n >>>\ny" },
    };

    public static TheoryData<string> Unchanged => new()
    {
        ">>>>",
        ">>>x",
        ">>> x",
        ">>> >>>",
        "<<<>>>",
        ">> >",
        UnicodeTexts.UnitSeparator + Open,
        Close + UnicodeTexts.CombiningAcute,
        UnicodeTexts.FullwidthGreaterThan + UnicodeTexts.FullwidthGreaterThan + UnicodeTexts.FullwidthGreaterThan,
        UnicodeTexts.LoneHigh + Close,
        Close + UnicodeTexts.LoneLow,
        string.Empty,
        "\n\n",
    };

    public static TheoryData<int> LineBreaks => new(FenceCodePoints.LineBreaks);

    public static TheoryData<int> Invisible => new(FenceCodePoints.Invisible);

    public static TheoryData<int> Padding => new(FenceCodePoints.Padding);

    public static TheoryData<int> Visible => new(FenceCodePoints.Visible);

    [Theory]
    [MemberData(nameof(LineBreaks))]
    public void SplitsLinesAtEveryBreakInTheSet(int codePoint)
    {
        var lineBreak = char.ConvertFromUtf32(codePoint);

        Assert.Equal("a" + lineBreak + " " + Close + lineBreak + "b", FenceNeutralizer.Neutralize("a" + lineBreak + Close + lineBreak + "b"));
    }

    [Theory]
    [MemberData(nameof(Invisible))]
    public void IgnoresEveryInvisibleCodePointAroundAMarker(int codePoint)
    {
        var invisible = char.ConvertFromUtf32(codePoint);

        Assert.Equal(" " + invisible + Close + invisible, FenceNeutralizer.Neutralize(invisible + Close + invisible));
    }

    [Theory]
    [MemberData(nameof(Padding))]
    public void TrimsEveryPaddingCodePointAroundAMarker(int codePoint)
    {
        var padding = char.ConvertFromUtf32(codePoint);

        Assert.Equal(" " + padding + Open + padding, FenceNeutralizer.Neutralize(padding + Open + padding));
    }

    [Theory]
    [MemberData(nameof(Visible))]
    public void KeepsAMarkerBesideAnyOtherCodePointAsIs(int codePoint)
    {
        var visible = char.ConvertFromUtf32(codePoint);

        Assert.Equal("a" + visible + Close + "\n" + visible + Open, FenceNeutralizer.Neutralize("a" + visible + Close + "\n" + visible + Open));
    }

    [Theory]
    [MemberData(nameof(Neutralized))]
    public void IndentsEveryLineThatReducesToAFenceMarker(string content, string expected) =>
        Assert.Equal(expected, FenceNeutralizer.Neutralize(content));

    [Theory]
    [MemberData(nameof(Unchanged))]
    public void LeavesEveryOtherLineAsIs(string content) => Assert.Equal(content, FenceNeutralizer.Neutralize(content));
}
