namespace CrucibleAI.Tests.Harnesses;

internal static class RepositorySchemas
{
    private const string Prefix = "repository-schemas/";

    public static string Text(string fileName)
    {
        using var stream = typeof(RepositorySchemas).Assembly.GetManifestResourceStream(Prefix + fileName)
            ?? throw new FileNotFoundException(fileName);
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: false);
        return reader.ReadToEnd();
    }
}
