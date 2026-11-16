namespace CrucibleAI.Tests.Harnesses;

internal sealed record SurfaceMapping(IReadOnlyList<string> Members, bool Present, string? NotInCSharp)
{
    public static SurfaceMapping To(bool present, params string[] members) => new(members, present, null);

    public static SurfaceMapping Absent(string reason) => new([], true, reason);
}
