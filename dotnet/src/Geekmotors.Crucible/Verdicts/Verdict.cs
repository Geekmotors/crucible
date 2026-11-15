namespace Geekmotors.Crucible.Verdicts;

internal sealed record Verdict(VerdictOutcome Outcome, int Need, int Passed, int Failed, int Errored);
