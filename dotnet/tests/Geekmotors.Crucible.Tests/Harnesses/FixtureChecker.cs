using System.Text.Json;
using Json.Schema;

namespace Geekmotors.Crucible.Tests.Harnesses;

internal sealed class FixtureChecker
{
    private readonly JsonSchema _schema;
    private readonly HashSet<string> _covered;

    public FixtureChecker(JsonSchema schema, IEnumerable<string> coveredOperations)
    {
        _schema = schema;
        _covered = coveredOperations.ToHashSet(StringComparer.Ordinal);
    }

    public IReadOnlyList<(string File, FixtureProblem Problem)> Check(IEnumerable<FixtureFile> files)
    {
        var problems = new List<(string, FixtureProblem)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            if (!IsValid(file))
            {
                problems.Add((file.Name, FixtureProblem.SchemaViolation));
            }
            if (!_covered.Contains(file.OperationKey))
            {
                problems.Add((file.Name, FixtureProblem.UncoveredOperation));
            }
            var prefix = $"{file.Area}-{file.Operation}-";
            foreach (var fixtureCase in file.Cases)
            {
                if (!fixtureCase.Id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    problems.Add((file.Name, FixtureProblem.IdPrefix));
                }
                if (!seen.Add(fixtureCase.Id))
                {
                    problems.Add((file.Name, FixtureProblem.DuplicateId));
                }
            }
        }
        return problems;
    }

    private bool IsValid(FixtureFile file)
    {
        using var document = JsonDocument.Parse(file.Text);
        return _schema.Evaluate(document.RootElement).IsValid;
    }
}
