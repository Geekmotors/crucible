using CrucibleAI.Urls;

namespace CrucibleAI.Providers;

internal sealed class PlainHttpRule
{
    private const string PlainScheme = "http";
    private const string LoopbackName = "localhost";
    private const string LoopbackIpv4Prefix = "127.";
    private const string LoopbackIpv6 = "[::1]";

    public bool Warns(HttpUrl baseUrl, bool keySent) => keySent && baseUrl.Scheme == PlainScheme && !IsLoopback(baseUrl);

    private static bool IsLoopback(HttpUrl url) => url.HostKind switch
    {
        HostKind.Domain => url.Host == LoopbackName,
        HostKind.Ipv4 => url.Host.StartsWith(LoopbackIpv4Prefix, StringComparison.Ordinal),
        HostKind.Ipv6 => url.Host == LoopbackIpv6,
        _ => false,
    };
}
