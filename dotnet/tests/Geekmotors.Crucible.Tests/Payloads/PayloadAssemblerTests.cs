using CrucibleAI.Payloads;
using CrucibleAI.Tests.Constants;

namespace CrucibleAI.Tests.Payloads;

public sealed class PayloadAssemblerTests
{
    [Fact]
    public void WithoutStateWritesTheSectionsInOrder() =>
        Assert.Equal(
            $"{PayloadTexts.Response}\n<<<\n{PayloadTexts.Reply}\n>>>\n\n{PayloadTexts.Claim}\n<<<\n{PayloadTexts.Expectation}\n>>>",
            PayloadAssembler.Assemble(
                [],
                [new PayloadSection(PayloadTexts.Response, PayloadTexts.Reply), new PayloadSection(PayloadTexts.Claim, PayloadTexts.Expectation)]));

    [Fact]
    public void StateComesFirstWithOneFactPerLineUntrimmed() =>
        Assert.Equal(
            $"STATE\n<<<\n{PayloadTexts.PaddedFact}\n\n>>>\n\n{PayloadTexts.Response}\n<<<\n{PayloadTexts.Reply}\n>>>",
            PayloadAssembler.Assemble([PayloadTexts.PaddedFact, string.Empty], [new PayloadSection(PayloadTexts.Response, PayloadTexts.Reply)]));

    [Fact]
    public void FencesInsideAFactAreNeutralized() =>
        Assert.Equal(
            $"STATE\n<<<\n {PayloadTexts.Fence}\n>>>\n\n{PayloadTexts.Response}\n<<<\n\n>>>",
            PayloadAssembler.Assemble([PayloadTexts.Fence], [new PayloadSection(PayloadTexts.Response, string.Empty)]));
}
