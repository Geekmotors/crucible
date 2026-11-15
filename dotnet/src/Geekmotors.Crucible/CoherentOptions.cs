namespace CrucibleAI;

/// <summary>Options of one <c>CoherentAsync</c> call.</summary>
/// <remarks>
/// New options (attachments, for example) are added as new properties, so existing initialisers keep compiling.
/// </remarks>
public sealed record CoherentOptions
{
    /// <summary>
    /// Facts sent with this call only, after the run's facts. They are not added to the run.
    /// <see langword="null"/>: the run's facts alone.
    /// </summary>
    public IReadOnlyList<string>? Facts { get; init; }
}
