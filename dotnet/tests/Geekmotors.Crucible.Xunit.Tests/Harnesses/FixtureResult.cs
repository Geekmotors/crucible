namespace CrucibleAI.Xunit.Tests.Harnesses;

internal sealed record FixtureResult(string DisplayName, FixtureOutcome Outcome, IReadOnlyList<string> ExceptionTypes, string? Message)
{
    public string? ExceptionType => ExceptionTypes.Count > 0 ? ExceptionTypes[0] : null;
}
