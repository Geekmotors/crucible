namespace CrucibleAI.Assertions;

internal static class FactList
{
    public static IReadOnlyList<string> Entries(IEnumerable<string?>? facts)
    {
        var entries = facts?.ToArray();
        if (entries is null || Array.Exists(entries, entry => entry is null))
        {
            throw CrucibleException.Usage("Facts take a string or a list of strings, never null.");
        }
        return entries!;
    }

    public static IReadOnlyList<string> Optional(IEnumerable<string?>? facts) => facts is null ? [] : Entries(facts);
}
