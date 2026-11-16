namespace Geekmotors.Crucible.Options;

internal sealed class RunOptionsValidator
{
    private static readonly RunOptions Undeclared = new();

    public RunOptions Validate(RunOptions? options, RunOptionsSource source)
    {
        if (options is null)
        {
            return source == RunOptionsSource.Config ? Undeclared : throw Rejection(source, "Run options must be given.");
        }
        if (options.Runs < 1)
        {
            throw Rejection(source, "\"runs\" must be an integer of at least 1.");
        }
        if (options.Threshold is { } threshold && !(threshold > 0 && threshold <= 1))
        {
            throw Rejection(source, "\"threshold\" must be a number greater than 0 and at most 1.");
        }
        if (options.InferenceTimeout < 1 || options.TestTimeout < 1)
        {
            throw Rejection(source, "Timeouts must be an integer number of milliseconds from 1 to 2147483647.");
        }
        return options;
    }

    public CrucibleException Rejection(RunOptionsSource source, string problem)
    {
        var message = $"Invalid run options in {Place(source)}: {problem}";
        return source == RunOptionsSource.Config ? CrucibleException.Config(message) : CrucibleException.Usage(message);
    }

    private static string Place(RunOptionsSource source) => source switch
    {
        RunOptionsSource.Test => "a test",
        RunOptionsSource.Group => "a group",
        RunOptionsSource.Config => "\"testDefaults\" of the config file",
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };
}
