namespace CrucibleAI.Payloads;

internal static class PayloadAssembler
{
    private const string StateHeader = "STATE";
    private const string Open = "<<<";
    private const string Close = ">>>";

    public static string Assemble(IReadOnlyList<string> state, IReadOnlyList<PayloadSection> sections)
    {
        var ordered = state.Count > 0 ? sections.Prepend(new PayloadSection(StateHeader, string.Join('\n', state))) : sections;
        return string.Join("\n\n", ordered.Select(Section));
    }

    private static string Section(PayloadSection section) =>
        $"{section.Header}\n{Open}\n{FenceNeutralizer.Neutralize(section.Content)}\n{Close}";
}
