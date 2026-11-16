using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

internal static partial class FactProbe
{
    private static readonly CompositeFormat Token = CompositeFormat.Parse(FixtureTexts.TokenFormat);

    public static string Own(string owner) => string.Format(CultureInfo.InvariantCulture, Token, owner, Crucible.Context.Run);

    public static void Add(string owner) => Crucible.Facts(Own(owner));

    public static async Task SeesOnlyItsOwnAsync(string owner)
    {
        var own = Own(owner);
        var prompt = await Crucible.ExtractAsync<string>(FixtureTexts.Response, FixtureTexts.Target) ?? string.Empty;

        Assert.Equal([own], Tokens().Matches(prompt).Select(match => match.Value).Distinct());
    }

    public static async Task AddAndCheckAsync(string owner)
    {
        Add(owner);
        await Task.Yield();
        await SeesOnlyItsOwnAsync(owner);
    }

    [GeneratedRegex(FixtureTexts.TokenPattern)]
    private static partial Regex Tokens();
}
