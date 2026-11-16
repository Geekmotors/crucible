namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class ErrorKindName
{
    public static string Of(CrucibleErrorKind kind) => kind switch
    {
        CrucibleErrorKind.Usage => "usage",
        CrucibleErrorKind.Config => "config",
        CrucibleErrorKind.Infra => "infra",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
