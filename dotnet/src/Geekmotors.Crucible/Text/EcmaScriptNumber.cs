using System.Globalization;
using System.Text;

namespace CrucibleAI.Text;

internal static class EcmaScriptNumber
{
    private const int PlainDigitLimit = 21;
    private const int SmallestPlainPoint = -5;

    public static string Format(double value)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }
        if (double.IsInfinity(value))
        {
            return value > 0 ? "Infinity" : "-Infinity";
        }
        if (value == 0)
        {
            return "0";
        }
        var (digits, point) = Shortest(Math.Abs(value));
        var sign = value < 0 ? "-" : string.Empty;
        return sign + Layout(digits, point);
    }

    private static string Layout(string digits, int point)
    {
        var count = digits.Length;
        if (count <= point && point <= PlainDigitLimit)
        {
            return digits + new string('0', point - count);
        }
        if (point > 0 && point <= PlainDigitLimit)
        {
            return digits[..point] + "." + digits[point..];
        }
        if (point >= SmallestPlainPoint && point <= 0)
        {
            return "0." + new string('0', -point) + digits;
        }
        var exponent = point - 1;
        var mantissa = count == 1 ? digits : digits[..1] + "." + digits[1..];
        return mantissa + "e" + (exponent < 0 ? "-" : "+") + Math.Abs(exponent).ToString(CultureInfo.InvariantCulture);
    }

    private static (string Digits, int Point) Shortest(double magnitude)
    {
        var roundTrip = magnitude.ToString("R", CultureInfo.InvariantCulture);
        var parts = roundTrip.Split('E');
        var exponent = parts.Length > 1 ? int.Parse(parts[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) : 0;
        var mantissa = parts[0];
        var dot = mantissa.IndexOf('.', StringComparison.Ordinal);
        var digits = new StringBuilder(dot < 0 ? mantissa : mantissa.Remove(dot, 1));
        var point = (dot < 0 ? mantissa.Length : dot) + exponent;
        while (digits[0] == '0')
        {
            digits.Remove(0, 1);
            point--;
        }
        while (digits[^1] == '0')
        {
            digits.Length--;
        }
        return (digits.ToString(), point);
    }
}
