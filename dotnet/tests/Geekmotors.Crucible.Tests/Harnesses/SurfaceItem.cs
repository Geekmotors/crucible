namespace CrucibleAI.Tests.Harnesses;

internal sealed record SurfaceItem(string Key, IReadOnlyList<string> Fixtures)
{
    public override string ToString() => Key;
}
