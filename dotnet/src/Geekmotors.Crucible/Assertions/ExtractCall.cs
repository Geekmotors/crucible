namespace Geekmotors.Crucible.Assertions;

internal sealed record ExtractCall(string? Text, string? Description, Type? As, bool? Strict, IEnumerable<string?>? Facts);
