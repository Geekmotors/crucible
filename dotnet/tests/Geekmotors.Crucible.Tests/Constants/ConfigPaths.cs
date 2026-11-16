namespace Geekmotors.Crucible.Tests.Constants;

internal static class ConfigPaths
{
    public const string ProjectDirectory = "/project";
    public const string JsonName = "crucible.config.json";
    public static readonly string JsonPath = Path.Combine(ProjectDirectory, JsonName);
    public const string Text = """{ "provider": "openrouter", "model": "judge-model" }""";
    public const string AbsentName = "no-such-crucible.config.json";
    public const string DependencyManifest = "Geekmotors.Crucible.Tests.deps.json";
    public const char JsonObjectStart = '{';
}
