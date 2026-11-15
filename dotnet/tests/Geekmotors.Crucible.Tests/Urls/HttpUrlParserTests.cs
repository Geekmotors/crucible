using CrucibleAI.Tests.Constants;
using CrucibleAI.Urls;

namespace CrucibleAI.Tests.Urls;

public sealed class HttpUrlParserTests
{
    private readonly HttpUrlParser _parser = new();

    public static TheoryData<string, string> Serialized => UrlCases.Serialized;

    public static TheoryData<string> Rejected => UrlCases.Rejected;

    public static TheoryData<string> WithCredentials => UrlCases.WithCredentials;

    [Theory]
    [MemberData(nameof(Serialized))]
    public void SerialisesAsTheWhatwgUrlStandardDoes(string input, string href) => Assert.Equal(href, _parser.Parse(input)?.Href);

    [Theory]
    [MemberData(nameof(Rejected))]
    public void RejectsWhatIsNotAnHttpUrl(string input) => Assert.Null(_parser.Parse(input));

    [Theory]
    [MemberData(nameof(WithCredentials))]
    public void TellsWhenTheUrlHoldsCredentials(string input) => Assert.True(_parser.Parse(input)?.HasCredentials);

    [Fact]
    public void APlainUrlHoldsNoCredentials() => Assert.False(_parser.Parse(UrlCases.PlainUrl)?.HasCredentials);
}
