using Geekmotors.Crucible.Judging;

namespace Geekmotors.Crucible.Runs;

internal sealed record ExtractionCall(
    string Description,
    ExtractType Type,
    ExtractMode Mode,
    object? Value,
    string Reasoning,
    string Response,
    IReadOnlyList<string> Facts) : JudgeCall;
