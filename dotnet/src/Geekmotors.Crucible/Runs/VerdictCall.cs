namespace CrucibleAI.Runs;

internal sealed record VerdictCall(string Claim, bool Verdict, string Reasoning, string Response, IReadOnlyList<string> Facts) : JudgeCall;
