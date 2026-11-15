using Geekmotors.Crucible.Assertions;

namespace Geekmotors.Crucible.Runs;

internal sealed class RunRecord
{
    private readonly object _gate = new();
    private readonly List<string> _facts = [];
    private readonly List<JudgeCall> _calls = [];

    public IReadOnlyList<string> Facts
    {
        get
        {
            lock (_gate)
            {
                return [.. _facts];
            }
        }
    }

    public IReadOnlyList<JudgeCall> Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    public void AddFacts(IEnumerable<string?>? facts)
    {
        var entries = FactList.Entries(facts);
        lock (_gate)
        {
            _facts.AddRange(entries);
        }
    }

    public void Record(JudgeCall call)
    {
        lock (_gate)
        {
            _calls.Add(call);
        }
    }
}
