namespace CrucibleAI.Tests.Constants;

internal static class UrlCases
{
    public static TheoryData<string, string> Serialized { get; } = new()
    {
        { " http://Example.COM/v1 ", "http://example.com/v1" },
        { "http://exa\tmple.com/v\n1", "http://example.com/v1" },
        { "http:\\\\example.com\\v1\\x", "http://example.com/v1/x" },
        { "http:example.com/v1", "http://example.com/v1" },
        { "https:////example.com/v1", "https://example.com/v1" },
        { "http://example.com/a b", "http://example.com/a%20b" },
        { "http://example.com/caf\u00E9", "http://example.com/caf%C3%A9" },
        { "http://example.com/%2e%2E/v1/.", "http://example.com/v1/" },
        { "http://example.com/a/..", "http://example.com/" },
        { "http://example.com/a/./b", "http://example.com/a/b" },
        { "http://ex%41mple.com/", "http://example.com/" },
        { "http://b\u00FCcher.example/", "http://xn--bcher-kva.example/" },
        { "http://0177.0.0.1/", "http://127.0.0.1/" },
        { "http://0x7f000001/", "http://127.0.0.1/" },
        { "http://127.1/", "http://127.0.0.1/" },
        { "http://[0:0:0:0:0:0:0:1]/", "http://[::1]/" },
        { "http://[::ffff:127.0.0.1]/", "http://[::ffff:7f00:1]/" },
        { "http://[1:0:0:2:0:0:0:3]/", "http://[1:0:0:2::3]/" },
        { "http://[1:2:3:4:5:6:7:8]/", "http://[1:2:3:4:5:6:7:8]/" },
        { "http://[ABCD::]/", "http://[abcd::]/" },
        { "http://example.com:080/", "http://example.com/" },
        { "http://example.com:/", "http://example.com/" },
        { "https://example.com:443", "https://example.com/" },
        { "https://example.com:8443", "https://example.com:8443/" },
        { "http://@example.com/", "http://example.com/" },
    };

    public static TheoryData<string> Rejected { get; } = new(
    [
        "http://",
        "http://:80/",
        "http://example.com:65536/",
        "http://example.com:8a/",
        "http://256.0.0.1/",
        "http://1.2.3.4.5/",
        "http://0x100000000/",
        "http://[::1/",
        "http://[1::2::3]/",
        "http://[1:2:3:4:5:6:7:8:9]/",
        "http://exa mple.com/",
        "http://exa%20mple.com/",
        "ftp://example.com/",
        "example.com/v1",
        "1http://example.com/",
        "http://example.com@/",
    ]);

    public static TheoryData<string> WithCredentials { get; } = new(
    [
        "http://user:pass@example.com/",
        "http://user@example.com/",
        "http://:pass@example.com/",
        "http://a@b@example.com/",
    ]);

    public const string PlainUrl = "http://example.com/v1";
}
