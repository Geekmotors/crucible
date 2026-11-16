using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Spec;

public sealed class SurfaceTests
{
    public static TheoryData<string> Keys => SurfaceManifest.Keys;

    public static TheoryData<string> PortedKeysWithFixtures => new(SurfaceManifest.Items
        .Where(item => item.Fixtures.Count > 0 && CSharpSurface.For(item.Key) is { NotInCSharp: null })
        .Select(item => item.Key));

    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryManifestItemExistsInTheCSharpApiOrIsLeftOutWithAReason(string key)
    {
        var mapping = CSharpSurface.For(key);

        Assert.NotNull(mapping);
        Assert.True(mapping.Present, key);
    }

    [Theory]
    [MemberData(nameof(PortedKeysWithFixtures))]
    public void EveryPortedItemIsProvenByAFixtureCaseThatRunsInCSharp(string key)
    {
        var running = SurfaceManifest.Item(key).Fixtures
            .Where(id => !SpecOperations.Skipped.ContainsKey(id))
            .Select(SpecFixtureCatalog.Case)
            .Where(entry => SpecOperations.Implemented.ContainsKey(entry.File.OperationKey));

        Assert.NotEmpty(running);
    }

    [Fact]
    public void NoMappingOutlivesItsManifestItem()
    {
        var manifest = SurfaceManifest.Items.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);

        var stale = CSharpSurface.MappedKeys.Where(key => !manifest.Contains(key)).ToArray();

        Assert.Empty(stale);
    }

    [Fact]
    public void EveryPublicMemberIsInTheManifestOrAKnownCSharpAddition()
    {
        var known = CSharpSurface.MappedMembers.Concat(CSharpSurface.CSharpAdditions).ToHashSet(StringComparer.Ordinal);

        var unlisted = CSharpSurface.PublicMembers.Where(member => !known.Contains(member)).ToArray();

        Assert.Empty(unlisted);
    }

    [Fact]
    public void EveryPublicTypeIsKnownToTheSurfaceCheck()
    {
        var exported = typeof(Crucible).Assembly.GetExportedTypes();

        Assert.Equal(
            CSharpSurface.PublicTypes.Select(type => type.FullName).Order(StringComparer.Ordinal),
            exported.Select(type => type.FullName).Order(StringComparer.Ordinal));
    }
}
