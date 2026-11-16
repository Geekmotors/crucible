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

    [Fact]
    public void WithAChatClientAMissingConfigFileGivesTheDefaults()
    {
        var settings = Loader(new FakeConfigFileSystem()).LoadRunSettings(new RecordingWarningSink());

        Assert.Equal(ConfigTexts.DefaultExcerptChars, settings.ExcerptChars);
    }

    [Fact]
    public void WithAChatClientTheConfigFileStillSuppliesTheRunSettings()
    {
        var settings = Loader(new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, ConfigTexts.ChatClientDefaults))
            .LoadRunSettings(new RecordingWarningSink());

        Assert.Equal((ConfigTexts.ChatClientRuns, ConfigTexts.ChatClientExcerptChars), (settings.TestDefaults.Runs, settings.ExcerptChars));
    }

    [Fact]
    public void WithAChatClientProviderFieldsAreIgnoredWithOneWarning()
    {
        var warnings = new RecordingWarningSink();

        Loader(new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, ConfigTexts.ProviderFieldsForAChatClient)).LoadRunSettings(warnings);

        Assert.Single(warnings.Lines);
    }

    [Fact]
    public void WithAChatClientAConfigWithoutProviderFieldsWarnsNothing()
    {
        var warnings = new RecordingWarningSink();

        Loader(new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, ConfigTexts.ChatClientDefaults)).LoadRunSettings(warnings);

        Assert.Empty(warnings.Lines);
    }

    [Theory]
    [InlineData(ConfigTexts.UnknownFieldForAChatClient)]
    [InlineData(ConfigTexts.KeyLikeFieldForAChatClient)]
    public void WithAChatClientAnInvalidConfigFileIsStillAConfigError(string text)
    {
        var loader = Loader(new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, text));

        var error = Assert.Throws<CrucibleException>(() => loader.LoadRunSettings(new RecordingWarningSink()));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    private static ConfigLoader Loader(FakeConfigFileSystem files) =>
        new(new ConfigFileLocator(files, () => ConfigPaths.ProjectDirectory), new FakeEnvironment());

    private static JudgeConfig Load(string text)
    {
        var files = new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, text);
        return new ConfigLoader(new ConfigFileLocator(files, () => ConfigPaths.ProjectDirectory), new FakeEnvironment()).Load();
    }
}
