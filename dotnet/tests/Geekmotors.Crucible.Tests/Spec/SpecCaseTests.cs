using Geekmotors.Crucible.Tests.Harnesses;

namespace Geekmotors.Crucible.Tests.Spec;

public sealed class SpecCaseTests
{
    public static TheoryData<string> ImplementedCases => SpecFixtureCatalog.CaseIds(SpecOperations.Implemented.Keys);

    [Theory]
    [MemberData(nameof(ImplementedCases))]
    public void Case(string id) => SpecCaseRunner.Run(id);
}
