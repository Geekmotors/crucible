namespace Geekmotors.Crucible;

/// <summary>What went wrong when Crucible throws a <see cref="CrucibleException"/>.</summary>
public enum CrucibleErrorKind
{
    /// <summary>A Crucible API was called incorrectly: invalid options or arguments, or a call outside a running test.</summary>
    Usage,

    /// <summary>The config file or the judge key environment variable is missing or invalid.</summary>
    Config,

    /// <summary>The judge provider or the judge's reply failed. See <see cref="CrucibleException.Retryable"/>.</summary>
    Infra,
}
