using Geekmotors.Crucible.Config;

namespace Geekmotors.Crucible.Tests.Fakes;

internal sealed class FakeEnvironment(IReadOnlyDictionary<string, string> variables) : IEnvironment
{
    public FakeEnvironment()
        : this(new Dictionary<string, string>())
    {
    }

    public string? Get(string name) => variables.TryGetValue(name, out var value) ? value : null;
}
