using System.Globalization;

namespace Geekmotors.Crucible.Urls;

internal sealed class HttpUrlParser
{
    private const string Http = "http";
    private const string Https = "https";
    private const int HttpPort = 80;
    private const int HttpsPort = 443;
    private const int HighestPort = 65535;
    private const string NoCredentials = ":";

    private static readonly char[] AuthorityEnds = ['/', '\\', '?', '#'];
    private static readonly char[] SegmentSeparators = ['/', '\\'];
    private static readonly char[] QueryOrFragment = ['?', '#'];
    private static readonly string[] SingleDot = [".", "%2e"];
    private static readonly string[] DoubleDot = ["..", ".%2e", "%2e.", "%2e%2e"];

    private readonly HostParser _hosts = new(new Ipv4Parser(), new Ipv6Parser());

    public HttpUrl? Parse(string input)
    {
        var text = Cleaned(input);
        var colon = text.IndexOf(':', StringComparison.Ordinal);
        if (colon < 0 || Scheme(text[..colon]) is not { } scheme)
        {
            return null;
        }
        var rest = text[(colon + 1)..].TrimStart(SegmentSeparators);
        var authorityEnd = rest.IndexOfAny(AuthorityEnds);
        var authority = authorityEnd < 0 ? rest : rest[..authorityEnd];
        var remaining = authorityEnd < 0 ? string.Empty : rest[authorityEnd..];
        if (remaining.IndexOfAny(QueryOrFragment) >= 0)
        {
            return null;
        }
        var at = authority.LastIndexOf('@');
        var credentials = at < 0 ? string.Empty : authority[..at];
        var hostAndPort = authority[(at + 1)..];
        if (SplitPort(hostAndPort) is not var (hostText, portText) || hostText.Length == 0)
        {
            return null;
        }
        var (validPort, port) = Port(portText, scheme);
        if (!validPort || _hosts.Parse(hostText) is not var (host, kind))
        {
            return null;
        }
        var hasCredentials = credentials.Length > 0 && credentials != NoCredentials;
        return new HttpUrl(scheme, host, kind, port, Path(remaining), hasCredentials);
    }

    private static string Cleaned(string input)
    {
        var trimmed = input.Trim(Enumerable.Range(0, 0x21).Select(unit => (char)unit).ToArray());
        return string.Concat(trimmed.Where(unit => unit is not ('\t' or '\n' or '\r')));
    }

    private static string? Scheme(string candidate)
    {
        if (candidate.Length == 0 || !char.IsAsciiLetter(candidate[0])
            || !candidate.All(unit => char.IsAsciiLetterOrDigit(unit) || unit is '+' or '-' or '.'))
        {
            return null;
        }
        var scheme = candidate.ToLowerInvariant();
        return scheme is Http or Https ? scheme : null;
    }

    private static (string Host, string? Port)? SplitPort(string hostAndPort)
    {
        var insideBrackets = false;
        for (var index = 0; index < hostAndPort.Length; index++)
        {
            switch (hostAndPort[index])
            {
                case '[':
                    insideBrackets = true;
                    break;
                case ']':
                    insideBrackets = false;
                    break;
                case ':' when !insideBrackets:
                    return (hostAndPort[..index], hostAndPort[(index + 1)..]);
            }
        }
        return (hostAndPort, null);
    }

    private static (bool Valid, int? Port) Port(string? text, string scheme)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (true, null);
        }
        if (!text.All(char.IsAsciiDigit))
        {
            return (false, null);
        }
        var trimmed = text.TrimStart('0');
        if (trimmed.Length > 5 || (trimmed.Length > 0 && int.Parse(trimmed, CultureInfo.InvariantCulture) > HighestPort))
        {
            return (false, null);
        }
        var port = trimmed.Length == 0 ? 0 : int.Parse(trimmed, CultureInfo.InvariantCulture);
        var defaultPort = scheme == Http ? HttpPort : HttpsPort;
        return (true, port == defaultPort ? null : port);
    }

    private static string Path(string remaining)
    {
        if (remaining.Length == 0)
        {
            return "/";
        }
        var segments = remaining[1..].Split(SegmentSeparators);
        var path = new List<string>();
        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            var isLast = index == segments.Length - 1;
            if (IsAny(segment, DoubleDot))
            {
                if (path.Count > 0)
                {
                    path.RemoveAt(path.Count - 1);
                }
                if (isLast)
                {
                    path.Add(string.Empty);
                }
            }
            else if (IsAny(segment, SingleDot))
            {
                if (isLast)
                {
                    path.Add(string.Empty);
                }
            }
            else
            {
                path.Add(PercentEncoding.EncodePathSegment(segment));
            }
        }
        return "/" + string.Join('/', path);
    }

    private static bool IsAny(string segment, string[] spellings) =>
        spellings.Any(spelling => string.Equals(segment, spelling, StringComparison.OrdinalIgnoreCase));
}
