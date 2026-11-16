using System.Globalization;

namespace CrucibleAI.Urls;

internal sealed class HostParser(Ipv4Parser ipv4, Ipv6Parser ipv6)
{
    private const string ForbiddenDomainCodePoints = " #%/:<>?@[\\]^|\u007F";

    private static readonly IdnMapping Idn = new() { AllowUnassigned = false, UseStd3AsciiRules = false };

    public (string Host, HostKind Kind)? Parse(string input)
    {
        if (input.StartsWith('['))
        {
            return input.EndsWith(']') && ipv6.Parse(input[1..^1]) is { } address ? (address, HostKind.Ipv6) : null;
        }
        if (DomainToAscii(PercentEncoding.Decode(input)) is not { } domain || domain.Any(IsForbidden))
        {
            return null;
        }
        if (ipv4.EndsInANumber(domain))
        {
            return ipv4.Parse(domain) is { } address ? (address, HostKind.Ipv4) : null;
        }
        return (domain, HostKind.Domain);
    }

    private static string? DomainToAscii(string domain)
    {
        if (domain.All(char.IsAscii))
        {
            return domain.Length == 0 ? null : domain.ToLowerInvariant();
        }
        try
        {
            return Idn.GetAscii(domain).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static bool IsForbidden(char unit) => unit < ' ' || ForbiddenDomainCodePoints.Contains(unit, StringComparison.Ordinal);
}
