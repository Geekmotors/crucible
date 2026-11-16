namespace CrucibleAI.Tests.Constants;

internal static class UnicodeTexts
{
    public static readonly string Grinning = char.ConvertFromUtf32(0x1F600);
    public static readonly string HighSurrogate = ((char)0xD83D).ToString();
    public static readonly string LoneHigh = ((char)0xD800).ToString();
    public static readonly string LoneLow = ((char)0xDC00).ToString();
    public static readonly string LineSeparator = ((char)0x2028).ToString();
    public static readonly string ParagraphSeparator = ((char)0x2029).ToString();
    public static readonly string NextLine = ((char)0x0085).ToString();
    public static readonly string VerticalTab = ((char)0x000B).ToString();
    public static readonly string FormFeed = ((char)0x000C).ToString();
    public static readonly string Escape = ((char)0x001B).ToString();
    public static readonly string Delete = ((char)0x007F).ToString();
    public static readonly string UnitSeparator = ((char)0x001F).ToString();
    public static readonly string NoBreakSpace = ((char)0x00A0).ToString();
    public static readonly string IdeographicSpace = ((char)0x3000).ToString();
    public static readonly string ZeroWidthSpace = ((char)0x200B).ToString();
    public static readonly string ByteOrderMark = ((char)0xFEFF).ToString();
    public static readonly string TagLetterA = char.ConvertFromUtf32(0xE0041);
    public static readonly string MusicalSymbolBeginBeam = char.ConvertFromUtf32(0x1D173);
    public static readonly string CombiningAcute = ((char)0x0301).ToString();
    public static readonly string FullwidthGreaterThan = ((char)0xFF1E).ToString();
    public static readonly string Ellipsis = ((char)0x2026).ToString();
    public static readonly string AccentedE = ((char)0x00E9).ToString();
}
