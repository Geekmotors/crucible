using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace CrucibleAI.Xunit;

internal sealed class CrucibleTheoryDiscoverer : TheoryDiscoverer
{
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute,
        ITheoryDataRow dataRow,
        object?[] testMethodArguments,
        string? index)
    {
        var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(
            discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments, index);
        IXunitTestCase testCase = new CrucibleTestCase(
            details.ResolvedTestMethod,
            details.TestCaseDisplayName,
            details.UniqueID,
            details.Explicit,
            dataRow.Label,
            dataRow.DisableParallelization ?? false,
            details.SkipExceptions,
            details.SkipReason,
            details.SkipType,
            details.SkipUnless,
            details.SkipWhen,
            TestIntrospectionHelper.GetTraits(testMethod, dataRow),
            testMethodArguments,
            details.SourceFilePath,
            details.SourceLineNumber,
            details.Timeout);
        return new([testCase]);
    }

    protected override async ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForTheory(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute)
    {
        var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
        if (details.SkipReason is not null && details.SkipUnless is null && details.SkipWhen is null)
        {
            return await base.CreateTestCasesForTheory(discoveryOptions, testMethod, theoryAttribute).ConfigureAwait(false);
        }
        return
        [
            new CrucibleDelayEnumeratedTestCase(
                details.ResolvedTestMethod,
                details.TestCaseDisplayName,
                details.UniqueID,
                details.Explicit,
                theoryAttribute.SkipTestWithoutData,
                details.SkipExceptions,
                details.SkipReason,
                details.SkipType,
                details.SkipUnless,
                details.SkipWhen,
                testMethod.Traits.ToDictionary(trait => trait.Key, trait => trait.Value.ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase),
                details.SourceFilePath,
                details.SourceLineNumber,
                details.Timeout),
        ];
    }
}
