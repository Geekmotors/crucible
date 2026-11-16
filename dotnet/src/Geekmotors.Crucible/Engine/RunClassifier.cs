namespace CrucibleAI.Engine;

internal sealed class RunClassifier
{
    public RunSettlement Classify(Exception? thrown)
    {
        if (thrown is null)
        {
            return RunSettlement.Passed;
        }
        if (thrown is CrucibleVerdictException)
        {
            return RunSettlement.Halt;
        }
        return FirstCrucible(thrown) switch
        {
            null => RunSettlement.Failed,
            { Kind: CrucibleErrorKind.Infra, Retryable: true } => RunSettlement.Errored,
            { Kind: CrucibleErrorKind.Infra } => RunSettlement.Fatal,
            _ => RunSettlement.Halt,
        };
    }

    private static CrucibleException? FirstCrucible(Exception thrown)
    {
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        for (var current = thrown; current is not null && visited.Add(current); current = current.InnerException)
        {
            if (current is CrucibleException crucible)
            {
                return crucible;
            }
        }
        return null;
    }
}
