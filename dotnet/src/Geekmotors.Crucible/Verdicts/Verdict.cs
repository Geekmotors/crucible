namespace CrucibleAI.Verdicts;

internal sealed record Verdict(VerdictZone Outcome, int Need, int Passed, int Failed, int Errored);
