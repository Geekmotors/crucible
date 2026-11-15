using CrucibleAI.Xunit.Fixture.Constants;
using CrucibleAI.Xunit.Fixture.Harnesses;

namespace CrucibleAI.Xunit.Fixture.Isolation;

public sealed class TheoryRowsTests
{
    public static TheoryData<UnserializableRow> UnserializableRows =>
        new(new UnserializableRow(FixtureTexts.Left), new UnserializableRow(FixtureTexts.Right));

    [CrucibleTheory(Runs = FixtureRuns.Few)]
    [InlineData(FixtureTexts.One)]
    [InlineData(FixtureTexts.Two)]
    [InlineData(FixtureTexts.Three)]
    public Task EachInlineRowSeesOnlyItsOwnFacts(string row) => FactProbe.AddAndCheckAsync(row);

    [CrucibleTheory(Runs = FixtureRuns.Few)]
    [ClassData(typeof(StringRows))]
    public Task EachClassDataRowSeesOnlyItsOwnFacts(string row) => FactProbe.AddAndCheckAsync(row);

    [CrucibleTheory(Runs = FixtureRuns.Few)]
    [MemberData(nameof(UnserializableRows))]
    public Task EachRowEnumeratedAtRunTimeSeesOnlyItsOwnFacts(UnserializableRow row) => FactProbe.AddAndCheckAsync(row.Name);
}
