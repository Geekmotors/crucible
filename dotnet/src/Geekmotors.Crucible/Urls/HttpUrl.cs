namespace Geekmotors.Crucible.Urls;

internal sealed record HttpUrl(string Scheme, string Host, HostKind HostKind, int? Port, string Path, bool HasCredentials)
{
    public string Href => Port is { } port ? $"{Scheme}://{Host}:{port}{Path}" : $"{Scheme}://{Host}{Path}";
}
