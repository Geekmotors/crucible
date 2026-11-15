using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Config;

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
}
