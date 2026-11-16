using System.Globalization;

namespace Geekmotors.Crucible.Urls;

internal sealed class Ipv4Parser
{
    private const int MaximumParts = 4;

    private static readonly int[] OctetShifts = [24, 16, 8, 0];

    public bool EndsInANumber(string host)
    {
        var parts = Parts(host);
        var last = parts[^1];
        if (last.Length == 0)
        {
            return false;
        }
        if (last.All(char.IsAsciiDigit))
        {
            return true;
        }
        return last.Length >= 2 && last[0] == '0' && last[1] is 'x' or 'X' && last[2..].All(char.IsAsciiHexDigit);
    }

    public string? Parse(string host)
    {
        var parts = Parts(host);
        if (parts.Count > MaximumParts)
        {
            return null;
        }
        var numbers = new List<ulong>();
        foreach (var part in parts)
        {
            if (Number(part) is not { } number)
            {
                return null;
            }
            numbers.Add(number);
        }
        if (numbers.SkipLast(1).Any(number => number > byte.MaxValue))
        {
            return null;
        }
        if (numbers[^1] >= 1UL << (8 * (MaximumParts + 1 - numbers.Count)))
        {
            return null;
        }
        var address = numbers[^1];
        for (var index = 0; index < numbers.Count - 1; index++)
        {
            address += numbers[index] << (8 * (MaximumParts - 1 - index));
        }
        var value = (uint)address;
        return string.Join('.', OctetShifts.Select(shift => ((value >> shift) & 0xFF).ToString(CultureInfo.InvariantCulture)));
    }

    private static List<string> Parts(string host)
    {
        var parts = host.Split('.').ToList();
        if (parts[^1].Length == 0 && parts.Count > 1)
        {
            parts.RemoveAt(parts.Count - 1);
        }
        return parts;
    }

    private static ulong? Number(string part)
    {
        if (part.Length == 0)
        {
            return null;
        }
        var radix = 10U;
        var digits = part;
        if (part.Length >= 2 && part[0] == '0' && part[1] is 'x' or 'X')
        {
            radix = 16U;
            digits = part[2..];
        }
        else if (part.Length >= 2 && part[0] == '0')
        {
            radix = 8;
            digits = part[1..];
        }
        var value = 0UL;
        foreach (var digit in digits)
        {
            var digitValue = char.IsAsciiHexDigit(digit) ? (uint)Convert.ToByte(digit.ToString(), 16) : uint.MaxValue;
            if (digitValue >= radix)
            {
                return null;
            }
            value = (value * radix) + digitValue;
            if (value > uint.MaxValue)
            {
                return null;
            }
        }
        return value;
    }
}
