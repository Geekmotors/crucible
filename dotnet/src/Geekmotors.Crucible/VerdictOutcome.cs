namespace CrucibleAI;

/// <summary>Why a gate did not pass, carried by <see cref="CrucibleVerdictException.Outcome"/>.</summary>
public enum VerdictOutcome
{
    /// <summary>The system under test missed the threshold: too few runs passed.</summary>
    Failed,

    /// <summary>
    /// Infrastructure prevented a verdict: judge provider failures, a fatal provider error or the test deadline.
    /// </summary>
    Errored,
}
