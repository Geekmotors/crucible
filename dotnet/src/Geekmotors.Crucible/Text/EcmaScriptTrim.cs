namespace Geekmotors.Crucible.Text;

internal static class EcmaScriptTrim
{
    private static readonly HashSet<char> WhiteSpace = ['\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0020', '\u00A0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000', '\uFEFF'];

    public static string Trim(string text)
    {
        var start = 0;
        var end = text.Length;
        while (start < end && IsWhiteSpace(text[start]))
        {
            start++;
        }
        while (end > start && IsWhiteSpace(text[end - 1]))
        {
            end--;
        }
        return text[start..end];
    }

    public static bool IsWhiteSpace(char unit) => WhiteSpace.Contains(unit);
}
