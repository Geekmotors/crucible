using System.Text;

namespace Geekmotors.Crucible.Payloads;

internal static class FenceNeutralizer
{
    private const int FenceLength = 3;

    private static readonly (int From, int To)[] LineBreaks =
    [
        (0x0A, 0x0D), (0x1C, 0x1E), (0x85, 0x85), (0x2028, 0x2029),
    ];

    private static readonly (int From, int To)[] Invisible =
    [
        (0xAD, 0xAD), (0x34F, 0x34F), (0x61C, 0x61C), (0x115F, 0x1160), (0x17B4, 0x17B5), (0x180B, 0x180F),
        (0x200B, 0x200F), (0x202A, 0x202E), (0x2060, 0x206F), (0x3164, 0x3164), (0xFE00, 0xFE0F), (0xFEFF, 0xFEFF),
        (0xFFA0, 0xFFA0), (0xFFF0, 0xFFF8), (0x1BCA0, 0x1BCA3), (0x1D173, 0x1D17A), (0xE0000, 0xE0FFF),
    ];

    private static readonly (int From, int To)[] Padding =
    [
        (0x09, 0x09), (0x20, 0x20), (0xA0, 0xA0), (0x1680, 0x1680), (0x2000, 0x200A), (0x202F, 0x202F), (0x205F, 0x205F),
        (0x3000, 0x3000),
    ];

    public static string Neutralize(string content)
    {
        var fenced = new StringBuilder(content.Length);
        var start = 0;
        for (var index = 0; index <= content.Length; index++)
        {
            if (index < content.Length && !In(LineBreaks, content[index]))
            {
                continue;
            }
            var line = content[start..index];
            if (IsFence(line))
            {
                fenced.Append(' ');
            }
            fenced.Append(line);
            if (index < content.Length)
            {
                fenced.Append(content[index]);
            }
            start = index + 1;
        }
        return fenced.ToString();
    }

    private static bool IsFence(string line)
    {
        var visible = CodePoints(line).Where(codePoint => !In(Invisible, codePoint)).ToList();
        var first = visible.FindIndex(codePoint => !In(Padding, codePoint));
        if (first < 0)
        {
            return false;
        }
        var last = visible.FindLastIndex(codePoint => !In(Padding, codePoint));
        var marker = visible.GetRange(first, last - first + 1);
        return marker.Count == FenceLength && (marker.TrueForAll(codePoint => codePoint == '<') || marker.TrueForAll(codePoint => codePoint == '>'));
    }

    private static IEnumerable<int> CodePoints(string line)
    {
        for (var index = 0; index < line.Length; index++)
        {
            if (char.IsHighSurrogate(line[index]) && index + 1 < line.Length && char.IsLowSurrogate(line[index + 1]))
            {
                yield return char.ConvertToUtf32(line[index], line[++index]);
                continue;
            }
            yield return line[index];
        }
    }

    private static bool In((int From, int To)[] ranges, int codePoint) =>
        Array.Exists(ranges, range => codePoint >= range.From && codePoint <= range.To);
}
