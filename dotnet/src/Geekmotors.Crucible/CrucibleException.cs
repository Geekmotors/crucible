namespace Geekmotors.Crucible;

/// <summary>
/// The error Crucible throws for a usage mistake, a configuration problem or a judge failure.
/// Branch on <see cref="Kind"/> (and <see cref="Retryable"/>), never on the message text.
/// </summary>
public sealed class CrucibleException : Exception
{
    private CrucibleException(CrucibleErrorKind kind, string message, bool? retryable, Exception? innerException)
        : base(message, innerException)
    {
        Kind = kind;
        Retryable = retryable;
    }

    /// <summary>What went wrong.</summary>
    public CrucibleErrorKind Kind { get; }

    /// <summary>
    /// Set only for <see cref="CrucibleErrorKind.Infra"/>: <see langword="true"/> when the failure is transient
    /// (the run is errored), <see langword="false"/> when the provider will refuse again (the whole test aborts).
    /// <see langword="null"/> for every other kind.
    /// </summary>
    public bool? Retryable { get; }

    internal static CrucibleException Usage(string message) => new(CrucibleErrorKind.Usage, message, null, null);

    internal static CrucibleException Config(string message) => new(CrucibleErrorKind.Config, message, null, null);

    internal static CrucibleException Infra(string message, bool retryable, Exception? innerException = null) =>
        new(CrucibleErrorKind.Infra, message, retryable, innerException);
}
