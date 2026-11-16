namespace Geekmotors.Crucible.Text;

internal static class TextFormat
{
    private const char LineSeparator = (char)0x2028;
    private const char ParagraphSeparator = (char)0x2029;

    public static string Cut(string text, int limit)
    {
        if (text.Length <= limit)
        {
            return text;
        }
        var cut = text[..limit];
        if (!char.IsHighSurrogate(cut[^1]))
        {
            return cut;
        }
        return limit > 1 ? cut[..^1] : text[..(limit + 1)];
    }

    public static IReadOnlyList<string> Lines(string value)
    {
        var lines = new List<string>();
        var start = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var unit = value[index];
            if (unit is not ('\r' or '\n' or LineSeparator or ParagraphSeparator))
            {
                continue;
            }
            lines.Add(value[start..index]);
            if (unit == '\r' && index + 1 < value.Length && value[index + 1] == '\n')
            {
                index++;
            }
            start = index + 1;
        }
        lines.Add(value[start..]);
        return lines;
    }

    public static string Indented(string value, string continuation) =>
        string.Join('\n', Lines(value).Select((line, index) => index == 0 || IsBlank(line) ? line : continuation + line));

    public static bool IsBlank(string line) => EcmaScriptTrim.Trim(line).Length == 0;
}
