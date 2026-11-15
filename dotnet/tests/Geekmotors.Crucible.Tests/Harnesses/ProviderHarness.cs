using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Providers;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed class ProviderHarness
{
    private static readonly Dictionary<string, string> DefaultKey = new(StringComparer.Ordinal)
    {
        [JudgeSettings.KeyVariable] = JudgeSettings.Key,
    };

    public ProviderHarness(string configText, IReadOnlyDictionary<string, string>? variables = null)
    {
        Environment = new FakeEnvironment(variables ?? DefaultKey);
        Config = Load(configText, Environment);
    }

    public FakeEnvironment Environment { get; }

    public JudgeConfig Config { get; }

    public RecordingWarningSink Warnings { get; } = new();

    public ChatCompletionsClient Client(params Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>[] answers) =>
        new(Config, new HttpClient(new ScriptedHandler(answers)), Environment, new ImmediateBackoff(), Warnings);

    public static JudgeConfig Load(string configText, IEnvironment environment)
    {
        var files = new FakeConfigFileSystem().WithFile(Path.Combine(ConfigFixtureHarness.ProjectDirectory, ConfigPaths.JsonName), configText);
        return new ConfigLoader(new ConfigFileLocator(files, () => ConfigFixtureHarness.ProjectDirectory), environment).Load();
    }

    public static JudgeRequest Request => new(JudgeSettings.System, JudgeSettings.Prompt);
}
