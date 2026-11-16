using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Fakes;

namespace Geekmotors.Crucible.Tests.Config;

public sealed class ConfigFileLocatorTests
{
    [Fact]
    public void ReadsTheFileInTheProjectDirectory()
    {
        var files = new FakeConfigFileSystem().WithFile(ConfigPaths.JsonPath, ConfigPaths.Text);

        Assert.Equal(new ConfigFileRead(ConfigFileState.Found, ConfigPaths.Text), Locator(files).Read(ConfigPaths.JsonName));
    }

    [Fact]
    public void AFileThatDoesNotExistIsMissing() =>
        Assert.Equal(ConfigFileState.Missing, Locator(new FakeConfigFileSystem()).Read(ConfigPaths.JsonName).State);

    [Fact]
    public void AFileThatExistsButCannotBeReadIsUnreadable()
    {
        var files = new FakeConfigFileSystem().WithUnreadable(ConfigPaths.JsonPath, new UnauthorizedAccessException());

        Assert.Equal(ConfigFileState.Unreadable, Locator(files).Read(ConfigPaths.JsonName).State);
    }

    [Fact]
    public void AFailedReadOfAFileThatIsGoneIsMissing()
    {
        var files = new FakeConfigFileSystem().WithFailingAbsent(ConfigPaths.JsonPath, new IOException());

        Assert.Equal(ConfigFileState.Missing, Locator(files).Read(ConfigPaths.JsonName).State);
    }

    [Fact]
    public void AMissingDirectoryOnThePathIsMissing()
    {
        var files = new FakeConfigFileSystem().WithUnreadable(ConfigPaths.JsonPath, new DirectoryNotFoundException());

        Assert.Equal(ConfigFileState.Missing, Locator(files).Read(ConfigPaths.JsonName).State);
    }

    private static ConfigFileLocator Locator(FakeConfigFileSystem files) => new(files, () => ConfigPaths.ProjectDirectory);
}
