namespace CrucibleAI.Tracing;

internal enum ProviderFailureNext
{
    Retry,
    Fatal,
    Exhausted,
    Aborted,
}
