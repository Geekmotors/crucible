namespace Geekmotors.Crucible.Tests.Harnesses;

internal static class SpecCaseRunner
{
    public static async Task RunAsync(string id)
    {
        if (SpecOperations.Skipped.TryGetValue(id, out var reason))
        {
            Assert.Skip(reason);
        }
        var (file, fixtureCase) = SpecFixtureCatalog.Case(id);
        var operation = SpecOperations.Implemented[file.OperationKey];
        var expect = SpecialValueDecoder.Decode(fixtureCase.Expect);
        var input = SpecialValueDecoder.Decode(fixtureCase.Input);
        var observed = await FixtureOutcome.CaptureAsync(() => operation.Run(input), operation.OutcomeKey, expect);
        Assert.True(
            JsonEquivalence.AreEquivalent(expect, observed),
            $"{id}: expected {JsonEquivalence.Print(expect)}, observed {JsonEquivalence.Print(observed)}");
    }
}
