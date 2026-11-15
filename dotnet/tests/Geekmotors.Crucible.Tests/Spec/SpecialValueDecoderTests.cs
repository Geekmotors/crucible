using System.Text.Json.Nodes;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Spec;

public sealed class SpecialValueDecoderTests
{
    [Fact]
    public void RejectsAnUnknownSpecialToken() =>
        Assert.Throws<UnknownSpecialTokenException>(() => SpecialValueDecoder.Decode(JsonNode.Parse(MalformedFixture.UnknownToken)));

    [Fact]
    public void DropsAnUnsetField()
    {
        var decoded = SpecialValueDecoder.Decode(JsonNode.Parse(MalformedFixture.UnsetField))!.AsObject();

        Assert.False(decoded.ContainsKey(OptionValues.RunsKey));
    }

    [Fact]
    public void DecodesNotANumber()
    {
        var decoded = SpecialValueDecoder.Decode(JsonNode.Parse(MalformedFixture.NotANumber))!;

        Assert.True(double.IsNaN(decoded.GetValue<double>()));
    }
}
