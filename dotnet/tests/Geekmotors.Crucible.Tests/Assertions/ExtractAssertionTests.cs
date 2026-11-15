using Geekmotors.Crucible.Assertions;
using Geekmotors.Crucible.Judging;
using Geekmotors.Crucible.Runs;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;
using Geekmotors.Crucible.Tests.Harnesses;

namespace Geekmotors.Crucible.Tests.Assertions;

public sealed class ExtractAssertionTests
{
    [Fact]
    public async Task ReturnsAndRecordsTheValueOfTheRequestedType()
    {
        var harness = new AssertionHarness(AssertionTexts.ExtractedNumber);

        var value = await harness.Extract.AskAsync(
            harness.Run(), new ExtractCall(AssertionTexts.Response, AssertionTexts.Target, typeof(double), null, null));

        var call = Assert.IsType<ExtractionCall>(Assert.Single(harness.Record.Calls));
        Assert.Equal(
            ((object?)AssertionTexts.ExtractedNumber, ExtractType.Number, ExtractMode.Lenient, (object?)AssertionTexts.ExtractedNumber),
            (value, call.Type, call.Mode, call.Value));
    }

    [Fact]
    public async Task SendsTheExtractSchemaAsTheSystemMessage()
    {
        var harness = new AssertionHarness(new EchoSystemJudgeClient());

        await harness.Extract.AskAsync(harness.Run(), new ExtractCall(AssertionTexts.Response, AssertionTexts.Target, null, null, null));

        Assert.Equal(RepositorySchemas.Text(AssertionTexts.ExtractSchema), Assert.IsType<ExtractionCall>(Assert.Single(harness.Record.Calls)).Reasoning);
    }

    [Fact]
    public async Task WithoutATypeExtractsAString()
    {
        var harness = new AssertionHarness();

        await harness.Extract.AskAsync(harness.Run(), new ExtractCall(AssertionTexts.Response, AssertionTexts.Target, null, null, null));

        Assert.Equal(ExtractType.String, Assert.IsType<ExtractionCall>(Assert.Single(harness.Record.Calls)).Type);
    }

    [Theory]
    [InlineData(true, null, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, null, false)]
    public async Task TheCallsStrictWinsOverTheTests(bool testStrict, bool? callStrict, bool expectedStrict)
    {
        var harness = new AssertionHarness();

        await harness.Extract.AskAsync(
            harness.Run(testStrict), new ExtractCall(AssertionTexts.Response, AssertionTexts.Target, null, callStrict, null));

        Assert.Equal(expectedStrict ? ExtractMode.Strict : ExtractMode.Lenient, Assert.IsType<ExtractionCall>(Assert.Single(harness.Record.Calls)).Mode);
    }

    [Fact]
    public async Task AnUnsupportedTypeIsAUsageError()
    {
        var harness = new AssertionHarness();

        var error = await Assert.ThrowsAsync<CrucibleException>(() => harness.Extract.AskAsync(
            harness.Run(), new ExtractCall(AssertionTexts.Response, AssertionTexts.Target, typeof(DateTime), null, null)));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task AMissingDescriptionIsAUsageError()
    {
        var harness = new AssertionHarness();

        var error = await Assert.ThrowsAsync<CrucibleException>(() => harness.Extract.AskAsync(
            harness.Run(), new ExtractCall(AssertionTexts.Response, null, null, null, null)));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }
}
