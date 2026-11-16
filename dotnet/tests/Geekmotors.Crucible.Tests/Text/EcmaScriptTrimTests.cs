using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Text;

namespace Geekmotors.Crucible.Tests.Text;

public sealed class EcmaScriptTrimTests
{
    [Fact]
    public void RemovesEveryEcmaScriptWhiteSpaceAroundTheText() =>
        Assert.Equal(
            TrimTexts.Word,
            EcmaScriptTrim.Trim(TrimTexts.EveryEcmaScriptSpace + TrimTexts.Word + TrimTexts.EveryEcmaScriptSpace));

    [Theory]
    [InlineData(TrimTexts.NextLine)]
    [InlineData(TrimTexts.MongolianVowelSeparator)]
    [InlineData(TrimTexts.FileSeparator)]
    public void KeepsCharactersEcmaScriptDoesNotTrim(string kept) =>
        Assert.Equal(kept + TrimTexts.Word + kept, EcmaScriptTrim.Trim(kept + TrimTexts.Word + kept));
}
