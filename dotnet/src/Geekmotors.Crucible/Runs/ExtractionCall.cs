using CrucibleAI.Judging;

namespace CrucibleAI.Runs;

internal sealed record ExtractionCall(
    string Description,
    ExtractType Type,
    ExtractMode Mode,
    object? Value,
    string Reasoning,
    string Response,
    IReadOnlyList<string> Facts) : JudgeCall;
