using Geekmotors.Crucible.Config;

namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class FakeConfigFileSystem : IConfigFileSystem
{
    private readonly Dictionary<string, string> _texts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Exception> _failures = new(StringComparer.Ordinal);
    private readonly HashSet<string> _present = new(StringComparer.Ordinal);

    public FakeConfigFileSystem WithFile(string path, string text)
    {
        _texts[path] = text;
        _present.Add(path);
        return this;
    }

    public FakeConfigFileSystem WithUnreadable(string path, Exception failure)
    {
        _failures[path] = failure;
        _present.Add(path);
        return this;
    }

    public FakeConfigFileSystem WithFailingAbsent(string path, Exception failure)
    {
        _failures[path] = failure;
        return this;
    }

    public string ReadText(string path)
    {
        if (_failures.TryGetValue(path, out var failure))
        {
            throw failure;
        }
        return _texts.TryGetValue(path, out var text) ? text : throw new FileNotFoundException(path);
    }

    public bool Exists(string path) => _present.Contains(path);
}
