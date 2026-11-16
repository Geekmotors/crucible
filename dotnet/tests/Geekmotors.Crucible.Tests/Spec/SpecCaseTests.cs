using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Spec;

public sealed class SpecCaseTests
{
    public static TheoryData<string> ImplementedCases => SpecFixtureCatalog.CaseIds(SpecOperations.Implemented.Keys);

    [Theory]
    [MemberData(nameof(ImplementedCases))]
    public Task Case(string id) => SpecCaseRunner.RunAsync(id);
}
