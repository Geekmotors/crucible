namespace CrucibleAI.Assertions;

internal sealed record CoherentCall(string? Actual, string? Expectation, IEnumerable<string?>? Facts);
