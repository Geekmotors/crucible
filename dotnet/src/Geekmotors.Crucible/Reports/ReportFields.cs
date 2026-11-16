using System.Globalization;
using Geekmotors.Crucible.Text;

namespace Geekmotors.Crucible.Reports;

internal sealed class ReportFields(int excerptChars)
{
    public const string Indent = "  ";
    public const string Nested = "    ";

    private const char Ellipsis = (char)0x2026;
    private const string FactSeparator = "\n";

    public string Field(string label, string value, string indent = Indent)
    {
        var text = indent + label + ": " + TextFormat.Indented(value, indent + Indent);
        return string.Join('\n', text.Split('\n').Select(line => line.TrimEnd(' ', '\t')));
    }

    public IEnumerable<string> Facts(IReadOnlyList<string> facts, string indent = Indent) =>
        facts.Count == 0 ? [] : [Bounded("Facts", string.Join(FactSeparator, facts), indent, FactSeparator)];

    public string Excerpt(string response, string indent = Indent) => Bounded("Response excerpt", response, indent, null);

    private string Bounded(string label, string value, string indent, string? separator)
    {
        if (value.Length <= excerptChars)
        {
            return Field(label, value, indent);
        }
        var cut = TextFormat.Cut(value, excerptChars);
        var endsOnWhole = separator is not null && value.AsSpan(cut.Length).StartsWith(separator, StringComparison.Ordinal);
        var bounded = string.Create(CultureInfo.InvariantCulture, $"{label} (first {cut.Length} of {value.Length} chars)");
        return Field(bounded, cut + (endsOnWhole ? separator : string.Empty) + Ellipsis, indent);
    }
}
