namespace Geekmotors.Crucible.Tracing;

internal enum ProviderFailureNext
{
    Retry,
    Fatal,
    Exhausted,
    Aborted,
}
