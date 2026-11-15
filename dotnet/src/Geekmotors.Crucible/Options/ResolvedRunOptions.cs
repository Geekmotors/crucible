namespace Geekmotors.Crucible.Options;

internal sealed record ResolvedRunOptions(int Runs, double Threshold, int InferenceTimeout, int TestTimeout, bool Strict)
{
    public ResolvedRunOptions ForCall(bool? strict) => strict is { } callStrict ? this with { Strict = callStrict } : this;
}
