using Geekmotors.Crucible.Json;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Json;

public sealed class JsonTextParserTests
{
    [Fact]
    public void StrictJsonRejectsComments() =>
        Assert.Throws<JsonSyntaxException>(() => JsonTextParser.Parse(JsonTexts.WithComment, JsonDialect.Strict));

    [Fact]
    public void StrictJsonRejectsTrailingCommas() =>
        Assert.Throws<JsonSyntaxException>(() => JsonTextParser.Parse(JsonTexts.WithTrailingComma, JsonDialect.Strict));

    [Fact]
    public void StrictJsonRejectsAByteOrderMark() =>
        Assert.Throws<JsonSyntaxException>(() => JsonTextParser.Parse(JsonTexts.WithByteOrderMark, JsonDialect.Strict));

    [Fact]
    public void StrictJsonReadsAnOverflowingNumberAsInfinity() =>
        Assert.Equal(double.PositiveInfinity, JsonTextParser.Parse(JsonTexts.Overflowing, JsonDialect.Strict)![0]!.GetValue<double>());

    [Fact]
    public void StrictJsonKeepsTheLastDuplicateKey() =>
        Assert.Equal(JsonTexts.LastDuplicateValue, JsonTextParser.Parse(JsonTexts.DuplicateKeys, JsonDialect.Strict)![JsonTexts.FirstKey]!.GetValue<double>());

    [Fact]
    public void ALineNumberCountsEveryKindOfLineBreak() =>
        Assert.Equal(
            JsonTexts.MixedLineBreaksErrorLine,
            Assert.Throws<JsonSyntaxException>(() => JsonTextParser.Parse(JsonTexts.ErrorAfterMixedLineBreaks, JsonDialect.Jsonc)).Line);

    [Fact]
    public void NestingUpToTheLimitParses() =>
        Assert.NotNull(JsonTextParser.Parse(Nested(JsonTexts.NestingLimit), JsonDialect.Strict));

    [Fact]
    public void NestingBeyondTheLimitIsASyntaxError() =>
        Assert.Throws<JsonSyntaxException>(() => JsonTextParser.Parse(Nested(JsonTexts.NestingLimit + 1), JsonDialect.Strict));

    private static string Nested(int depth) => new string(JsonTexts.OpenArray, depth) + new string(JsonTexts.CloseArray, depth);
}
