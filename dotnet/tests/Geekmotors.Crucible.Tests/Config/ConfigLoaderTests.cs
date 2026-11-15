using CrucibleAI.Config;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;

namespace CrucibleAI.Tests.Config;

public sealed class ConfigLoaderTests
{
    [Fact]
    public void AnExcerptLengthBeyondThirtyTwoBitsIsAConfigError()
    {
        var files = new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, ConfigTexts.ExcerptCharsBeyondThirtyTwoBits);
        var loader = new ConfigLoader(new ConfigFileLocator(files, () => ConfigPaths.ProjectDirectory), new FakeEnvironment());

        var error = Assert.Throws<CrucibleException>(loader.Load);

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    [Fact]
    public void KeepsTheTestDefaultsAsWrittenForTheTrace()
    {
        var config = Load(ConfigTexts.TestDefaultsInWrittenOrder);

        Assert.Equal(ConfigTexts.TestDefaultsCompact, config.TestDefaultsNode.ToJsonString());
    }

    [Fact]
    public void WithoutTestDefaultsTheTraceShowsAnEmptyObject()
    {
        var config = Load(ConfigTexts.WithoutTestDefaults);

        Assert.Equal(ConfigTexts.EmptyObject, config.TestDefaultsNode.ToJsonString());
    }

    private static JudgeConfig Load(string text)
    {
        var files = new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, text);
        return new ConfigLoader(new ConfigFileLocator(files, () => ConfigPaths.ProjectDirectory), new FakeEnvironment()).Load();
    }
}
