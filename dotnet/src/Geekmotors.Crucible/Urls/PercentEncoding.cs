using System.Globalization;
using System.Text;

namespace Geekmotors.Crucible.Urls;

internal static class PercentEncoding
{
    private const string PathEncoded = " \"#<>?^`{}";
    private static readonly UTF8Encoding Utf8 = new(false, false);

    public static string EncodePathSegment(string segment)
    {
        var encoded = new StringBuilder();
        foreach (var unit in Utf8.GetBytes(segment))
        {
            if (unit < 0x20 || unit > 0x7E || PathEncoded.Contains((char)unit, StringComparison.Ordinal))
            {
                encoded.Append('%').Append(unit.ToString("X2", CultureInfo.InvariantCulture));
            }
            else
            {
                encoded.Append((char)unit);
            }
        }
        return encoded.ToString();
    }

    public static string Decode(string text)
    {
        var bytes = Utf8.GetBytes(text);
        var decoded = new List<byte>(bytes.Length);
        for (var index = 0; index < bytes.Length; index++)
        {
            if (bytes[index] == '%' && index + 2 < bytes.Length && IsHex(bytes[index + 1]) && IsHex(bytes[index + 2]))
            {
                decoded.Add(byte.Parse(Encoding.ASCII.GetString(bytes, index + 1, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture));
                index += 2;
            }
            else
            {
                decoded.Add(bytes[index]);
            }
        }
        return Utf8.GetString(decoded.ToArray());
    }

    private static bool IsHex(byte unit) => char.IsAsciiHexDigit((char)unit);
}
