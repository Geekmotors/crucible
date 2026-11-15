using System.Reflection;
using Json.Schema;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class SpecFixtureCatalog
{
    private const string Prefix = "spec/";
    private const string SchemaName = "spec/fixture.schema.json";

    private static readonly Lazy<IReadOnlyList<FixtureFile>> LoadedFiles = new(Load);
    private static readonly Lazy<JsonSchema> LoadedSchema = new(() => JsonSchema.FromText(Read(SchemaName)));

    public static IReadOnlySet<string> SharedAssets { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        SchemaName,
        "spec/surface.json",
        "spec/surface.schema.json",
    };

    public static IReadOnlyList<FixtureFile> Files => LoadedFiles.Value;

    public static JsonSchema Schema => LoadedSchema.Value;

    public static IReadOnlyList<string> EmbeddedSpecFiles =>
        Assembly.GetManifestResourceNames()
            .Select(Normalized)
            .Where(path => path.StartsWith(Prefix, StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

    public static (FixtureFile File, FixtureCase Case) Case(string id) =>
        Files.SelectMany(file => file.Cases.Select(fixtureCase => (file, fixtureCase)))
            .Single(entry => entry.fixtureCase.Id == id);

    public static TheoryData<string> CaseIds(IEnumerable<string> operationKeys)
    {
        var operations = operationKeys.ToHashSet(StringComparer.Ordinal);
        var ids = Files
            .Where(file => operations.Contains(file.OperationKey))
            .SelectMany(file => file.Cases)
            .Select(fixtureCase => fixtureCase.Id);
        return new TheoryData<string>(ids);
    }

    private static List<FixtureFile> Load() =>
        Assembly.GetManifestResourceNames()
            .Where(name => Normalized(name).StartsWith(Prefix, StringComparison.Ordinal))
            .Where(name => !SharedAssets.Contains(Normalized(name)))
            .OrderBy(Normalized, StringComparer.Ordinal)
            .Select(name => FixtureFile.Parse(Normalized(name), Read(name)))
            .ToList();

    private static string Normalized(string resource) => resource.Replace('\\', '/');

    private static Assembly Assembly => typeof(SpecFixtureCatalog).Assembly;

    private static string Read(string resource)
    {
        using var stream = Assembly.GetManifestResourceStream(resource)
            ?? throw new FileNotFoundException(resource);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
