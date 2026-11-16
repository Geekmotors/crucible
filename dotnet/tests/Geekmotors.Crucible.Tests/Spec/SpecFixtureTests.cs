using Geekmotors.Crucible.Tests.Harnesses;

namespace Geekmotors.Crucible.Tests.Spec;

public sealed class SpecFixtureTests
{
    [Fact]
    public void EveryFixtureFileIsLoaded() => Assert.NotEmpty(SpecFixtureCatalog.Files);

    [Fact]
    public void EveryEmbeddedSpecFileIsAFixtureOrASharedAsset()
    {
        var accounted = SpecFixtureCatalog.Files.Select(file => file.Name).Concat(SpecFixtureCatalog.SharedAssets).Order(StringComparer.Ordinal);

        Assert.Equal(accounted, SpecFixtureCatalog.EmbeddedSpecFiles);
    }

    [Fact]
    public void EveryFixtureFileFollowsTheSchemaAndIdRulesAndIsCovered()
    {
        var checker = new FixtureChecker(SpecFixtureCatalog.Schema, SpecOperations.Covered);

        Assert.Empty(checker.Check(SpecFixtureCatalog.Files));
    }

    [Fact]
    public void EverySpecialValueInTheSpecIsKnown()
    {
        var cases = SpecFixtureCatalog.Files.SelectMany(file => file.Cases).ToList();

        Assert.All(cases, fixtureCase =>
        {
            SpecialValueDecoder.Decode(fixtureCase.Input);
            SpecialValueDecoder.Decode(fixtureCase.Expect);
        });
    }

    [Fact]
    public void ImplementedAndPendingOperationsDoNotOverlap() =>
        Assert.Empty(SpecOperations.Implemented.Keys.Intersect(SpecOperations.Pending));

    [Fact]
    public void EveryPendingOperationHasFixtures()
    {
        var operations = SpecFixtureCatalog.Files.Select(file => file.OperationKey).ToHashSet(StringComparer.Ordinal);

        Assert.All(SpecOperations.Pending, operation => Assert.Contains(operation, operations));
    }

    [Fact]
    public void EverySkippedCaseExists()
    {
        var ids = SpecFixtureCatalog.Files.SelectMany(file => file.Cases).Select(fixtureCase => fixtureCase.Id).ToHashSet();

        Assert.All(SpecOperations.Skipped.Keys, id => Assert.Contains(id, ids));
    }
}
