using System.Text;

namespace CrucibleAI.Payloads;

internal static class JudgeSchemas
{
    private static readonly Lazy<string> CoherentText = new(() => Read("coherent.md"));
    private static readonly Lazy<string> ExtractText = new(() => Read("extract.md"));

    public static string Coherent => CoherentText.Value;

    public static string Extract => ExtractText.Value;

    private static string Read(string fileName)
    {
        using var stream = typeof(JudgeSchemas).Assembly.GetManifestResourceStream("schemas/" + fileName)
            ?? throw new InvalidOperationException($"The judge schema {fileName} is missing from the assembly.");
        using var reader = new StreamReader(stream, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: false);
        return reader.ReadToEnd();
    }
}
