namespace CrucibleAI.Providers;

internal sealed class ProviderAttemptException(string message, bool fatal, Exception? innerException = null)
    : Exception(message, innerException)
{
    public bool Fatal { get; } = fatal;
}
