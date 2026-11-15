namespace CrucibleAI;

/// <summary>Options of one <c>ExtractAsync</c> call. The type to extract is the call's type argument.</summary>
/// <remarks>
/// New options (attachments, for example) are added as new properties, so existing initialisers keep compiling.
/// </remarks>
public sealed record ExtractOptions
{
    /// <summary>
    /// <see langword="true"/>: any hedge in the text extracts as <see langword="null"/>; <see langword="false"/>: the judge takes
    /// the value the text leans to. <see langword="null"/>: the test's resolved <c>Strict</c> option.
    /// </summary>
    public bool? Strict { get; init; }

    /// <summary>
    /// Facts sent with this call only, after the run's facts. They are not added to the run.
    /// <see langword="null"/>: the run's facts alone.
    /// </summary>
    public IReadOnlyList<string>? Facts { get; init; }
}
