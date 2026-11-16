using System.Text.Json.Nodes;
using Geekmotors.Crucible.Options;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Options;

public sealed class RunOptionsReaderTests
{
    private readonly RunOptionsReader _reader = new(new RunOptionsValidator());

    [Fact]
    public void ReadsRunsBuiltAsAnInteger()
    {
        var options = new JsonObject { [OptionValues.RunsKey] = JsonValue.Create(OptionValues.Runs) };

        Assert.Equal(OptionValues.Runs, _reader.Read(options, RunOptionsSource.Config).Runs);
    }

    [Fact]
    public void ReadsRunsBuiltAsADecimal()
    {
        var options = new JsonObject { [OptionValues.RunsKey] = JsonValue.Create((decimal)OptionValues.Runs) };

        Assert.Equal(OptionValues.Runs, _reader.Read(options, RunOptionsSource.Config).Runs);
    }

    [Fact]
    public void ReadsAWholeNumberWrittenWithAFraction() =>
        Assert.Equal(OptionValues.Runs, _reader.Read(JsonNode.Parse(OptionValues.RunsWithZeroFraction), RunOptionsSource.Config).Runs);

    [Fact]
    public void RejectsRunsBeyondThirtyTwoBitsWithTheSourceKind()
    {
        var options = new JsonObject { [OptionValues.RunsKey] = JsonValue.Create(OptionValues.BeyondThirtyTwoBits) };

        var error = Assert.Throws<CrucibleException>(() => _reader.Read(options, RunOptionsSource.Config));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    [Fact]
    public void RejectsAThresholdBeyondDoubleRangeAsConfig()
    {
        var error = Assert.Throws<CrucibleException>(
            () => _reader.Read(JsonNode.Parse(OptionValues.ThresholdBeyondDoubleRange), RunOptionsSource.Config));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    [Fact]
    public void RejectsAnUnknownKeyHoldingNullAsConfig()
    {
        var error = Assert.Throws<CrucibleException>(
            () => _reader.Read(JsonNode.Parse(OptionValues.UnknownKeyHoldingNull), RunOptionsSource.Config));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }
}
