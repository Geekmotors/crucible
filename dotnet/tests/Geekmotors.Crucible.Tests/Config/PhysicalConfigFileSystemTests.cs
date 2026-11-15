using Geekmotors.Crucible.Config;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Config;

public sealed class PhysicalConfigFileSystemTests
{
    private static readonly string OutputDirectory = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    private readonly PhysicalConfigFileSystem _files = new();

    [Fact]
    public void ReadsTheTextOfAFile() =>
        Assert.StartsWith(ConfigPaths.JsonObjectStart.ToString(), _files.ReadText(Path.Combine(OutputDirectory, ConfigPaths.DependencyManifest)));

    [Fact]
    public void ADirectoryExistsButCannotBeReadAsAConfigFile()
    {
        var locator = new ConfigFileLocator(_files, () => Path.GetDirectoryName(OutputDirectory)!);

        Assert.Equal(ConfigFileState.Unreadable, locator.Read(Path.GetFileName(OutputDirectory)).State);
    }

    [Fact]
    public void AnAbsentFileIsMissing()
    {
        var locator = new ConfigFileLocator(_files, () => OutputDirectory);

        Assert.Equal(ConfigFileState.Missing, locator.Read(ConfigPaths.AbsentName).State);
    }
}
