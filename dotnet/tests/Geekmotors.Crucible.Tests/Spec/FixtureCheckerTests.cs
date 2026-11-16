using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Spec;

public sealed class FixtureCheckerTests
{
    private readonly FixtureChecker _checker = new(SpecFixtureCatalog.Schema, [SpecOperations.VerdictCompute]);

    [Fact]
    public void AcceptsAWellFormedFile() =>
        Assert.Empty(_checker.Check([FixtureFile.Parse(MalformedFixture.FileName, MalformedFixture.Valid)]));

    [Fact]
    public void RejectsAMistypedInput() =>
        Assert.Contains(FixtureProblem.SchemaViolation, Problems(MalformedFixture.MistypedInput));

    [Fact]
    public void RejectsAnIdWithoutItsOperationPrefix() =>
        Assert.Contains(FixtureProblem.IdPrefix, Problems(MalformedFixture.WrongPrefix));

    [Fact]
    public void RejectsAnIdUsedTwice()
    {
        var problems = _checker.Check([
            FixtureFile.Parse(MalformedFixture.FileName, MalformedFixture.Valid),
            FixtureFile.Parse(MalformedFixture.OtherFileName, MalformedFixture.Valid),
        ]);

        Assert.Contains(problems, entry => entry.Problem == FixtureProblem.DuplicateId);
    }

    [Fact]
    public void RejectsAnOperationNeitherImplementedNorPending()
    {
        var checker = new FixtureChecker(SpecFixtureCatalog.Schema, [SpecOperations.RunOptionsResolve]);

        var problems = checker.Check([FixtureFile.Parse(MalformedFixture.FileName, MalformedFixture.Valid)]);

        Assert.Contains(problems, entry => entry.Problem == FixtureProblem.UncoveredOperation);
    }

    [Fact]
    public void ReportsACaseWithoutAnIdInsteadOfFailingToLoad() =>
        Assert.Contains(FixtureProblem.SchemaViolation, Problems(MalformedFixture.CaseWithoutId));

    private IEnumerable<FixtureProblem> Problems(string text) =>
        _checker.Check([FixtureFile.Parse(MalformedFixture.FileName, text)]).Select(entry => entry.Problem);
}
