using System.Text.Json.Nodes;

namespace CrucibleAI.Tests.Harnesses;

internal static class SpecCaseRunner
{
    public static async Task RunAsync(string id)
    {
        var (file, fixtureCase) = SpecFixtureCatalog.Case(id);
        var operation = SpecOperations.Implemented[file.OperationKey];
        var expect = SpecialValueDecoder.Decode(fixtureCase.Expect);
        var input = SpecialValueDecoder.Decode(fixtureCase.Input);
        if (SpecOperations.Skipped.TryGetValue(id, out var reason))
        {
            Assert.False(await PassesAsync(operation, input, expect), $"{id} is skipped but passes: remove its skip.");
            Assert.Skip(reason);
        }
        var observed = await FixtureOutcome.CaptureAsync(() => operation.Run(input), operation.OutcomeKey, expect);
        Assert.True(
            JsonEquivalence.AreEquivalent(expect, observed),
            $"{id}: expected {JsonEquivalence.Print(expect)}, observed {JsonEquivalence.Print(observed)}");
    }

    private static async Task<bool> PassesAsync(SpecOperation operation, JsonNode? input, JsonNode? expect)
    {
        try
        {
            var observed = await FixtureOutcome.CaptureAsync(() => operation.Run(input), operation.OutcomeKey, expect);
            return JsonEquivalence.AreEquivalent(expect, observed);
        }
        catch (InexpressibleInputException)
        {
            return false;
        }
    }
}
