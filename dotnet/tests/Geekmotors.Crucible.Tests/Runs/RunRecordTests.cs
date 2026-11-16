using CrucibleAI.Runs;
using CrucibleAI.Tests.Constants;

namespace CrucibleAI.Tests.Runs;

public sealed class RunRecordTests
{
    [Fact]
    public void KeepsFactsInTheOrderTheyWereAdded()
    {
        var record = new RunRecord();

        record.AddFacts([AssertionTexts.RunFact]);
        record.AddFacts([AssertionTexts.CallFact, AssertionTexts.LateFact]);

        Assert.Equal([AssertionTexts.RunFact, AssertionTexts.CallFact, AssertionTexts.LateFact], record.Facts);
    }

    [Fact]
    public void AFactsCallHoldingNullIsAUsageErrorAndAddsNothing()
    {
        var record = new RunRecord();

        var error = Assert.Throws<CrucibleException>(() => record.AddFacts([AssertionTexts.RunFact, null]));

        Assert.Equal((CrucibleErrorKind.Usage, 0), (error.Kind, record.Facts.Count));
    }

    [Fact]
    public void ANullFactsListIsAUsageError() =>
        Assert.Equal(CrucibleErrorKind.Usage, Assert.Throws<CrucibleException>(() => new RunRecord().AddFacts(null)).Kind);
}
