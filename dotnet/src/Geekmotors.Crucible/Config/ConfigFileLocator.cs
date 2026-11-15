namespace CrucibleAI.Config;

internal sealed class ConfigFileLocator(IConfigFileSystem files, Func<string> directory)
{
    public ConfigFileLocator()
        : this(new PhysicalConfigFileSystem(), Directory.GetCurrentDirectory)
    {
    }

    public string Location(string name) => Path.Combine(directory(), name);

    public ConfigFileRead Read(string name)
    {
        var path = Location(name);
        try
        {
            return new ConfigFileRead(ConfigFileState.Found, files.ReadText(path));
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException)
        {
            var absent = failure is FileNotFoundException or DirectoryNotFoundException || !files.Exists(path);
            return new ConfigFileRead(absent ? ConfigFileState.Missing : ConfigFileState.Unreadable, string.Empty);
        }
    }
}
