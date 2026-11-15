namespace CrucibleAI;

/// <summary>
/// Thrown by a gate that did not pass. <see cref="Exception.Message"/> is Crucible's failure report;
/// <see cref="Exception.InnerException"/> is the exception of a representative run, when there is one.
/// </summary>
public sealed class CrucibleVerdictException : Exception
{
    internal CrucibleVerdictException(VerdictOutcome outcome, string report, Exception? cause)
        : base(report, cause)
    {
        Outcome = outcome;
    }

    /// <summary>Whether the system under test failed the gate or infrastructure prevented a verdict.</summary>
    public VerdictOutcome Outcome { get; }
}
