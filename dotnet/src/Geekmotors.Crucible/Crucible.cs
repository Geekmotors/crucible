using CrucibleAI.Engine;
using CrucibleAI.Providers;

namespace CrucibleAI;

/// <summary>
/// Crucible's entry point: <see cref="Gate(RunOptions, Func{CrucibleContext, Task})"/> runs a test body many times and
/// passes when enough runs pass; the other members reach the run they are called from.
/// </summary>
public static class Crucible
{
    private static readonly RunScope Scope = new();
    private static readonly Lazy<GateEngine> LazyEngine = new(() => new GateEngine(_judge));
    private static IJudgeClient? _judge;

    /// <summary>
    /// The context of the run this call belongs to, wherever the run's async flow reaches.
    /// </summary>
    /// <exception cref="CrucibleException">A <see cref="CrucibleErrorKind.Usage"/> error outside a running gate body.</exception>
    public static CrucibleContext Context => Scope.Current.Context;

    /// <summary>
    /// Runs <paramref name="body"/> with the default run options (10 runs, threshold 0.9, unless the config file's
    /// <c>testDefaults</c> say otherwise). See <see cref="Gate(RunOptions, Func{CrucibleContext, Task})"/>.
    /// </summary>
    /// <param name="body">The test body; each run receives its own <see cref="CrucibleContext"/>.</param>
    /// <returns>A task that completes when the gate passed.</returns>
    /// <exception cref="CrucibleVerdictException">The gate did not pass.</exception>
    /// <exception cref="CrucibleException">A usage or configuration error, rethrown unchanged.</exception>
    public static Task Gate(Func<CrucibleContext, Task> body) => Engine.GateAsync(new RunOptions(), body);

    /// <summary>
    /// Runs <paramref name="body"/> <see cref="RunOptions.Runs"/> times at once, each run with its own facts, and passes
    /// when at least <see cref="RunOptions.Threshold"/> of the runs pass. A run passes when its body returns, is
    /// errored when it throws an <see cref="CrucibleErrorKind.Infra"/> error, and fails when it throws anything else.
    /// Works inside any test framework's test method.
    /// </summary>
    /// <param name="options">Run options; each one left <see langword="null"/> comes from the config file's <c>testDefaults</c>, else its default.</param>
    /// <param name="body">The test body; each run receives its own <see cref="CrucibleContext"/>.</param>
    /// <returns>A task that completes when the gate passed.</returns>
    /// <exception cref="CrucibleVerdictException">
    /// The gate did not pass; the message is the failure report. A gate nested in a run body that does not pass ends
    /// the outer gate too: its exception is rethrown unchanged.
    /// </exception>
    /// <exception cref="CrucibleException">
    /// <see cref="CrucibleErrorKind.Usage"/> for invalid options or a missing body (thrown by the call itself), for a
    /// body that returns a <see langword="null"/> task, and any usage or configuration error a run
    /// throws, rethrown unchanged after the other runs are cancelled; <see cref="CrucibleErrorKind.Config"/> for an invalid config file.
    /// </exception>
    public static Task Gate(RunOptions options, Func<CrucibleContext, Task> body) => Engine.GateAsync(options, body);

    /// <summary>
    /// Runs <paramref name="body"/> with the default run options, ending early when <paramref name="cancellationToken"/>
    /// is cancelled. See <see cref="Gate(RunOptions, Func{CrucibleContext, Task}, CancellationToken)"/>.
    /// </summary>
    /// <param name="body">The test body; each run receives its own <see cref="CrucibleContext"/>.</param>
    /// <param name="cancellationToken">Cancels the gate, typically the test framework's test cancellation token.</param>
    /// <returns>A task that completes when the gate passed.</returns>
    /// <exception cref="CrucibleVerdictException">The gate did not pass.</exception>
    /// <exception cref="CrucibleException">A usage or configuration error, rethrown unchanged.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled before the gate ended.</exception>
    public static Task Gate(Func<CrucibleContext, Task> body, CancellationToken cancellationToken) =>
        Engine.GateAsync(new RunOptions(), body, cancellationToken);

    /// <summary>
    /// Runs <paramref name="body"/> as <see cref="Gate(RunOptions, Func{CrucibleContext, Task})"/> does, ending early when
    /// <paramref name="cancellationToken"/> is cancelled: the gate stops waiting for its runs, cancels every run's
    /// <see cref="CrucibleContext.CancellationToken"/> and in-flight judge calls, and throws
    /// <see cref="OperationCanceledException"/>. Pass the test framework's test cancellation token (NUnit's
    /// <c>TestContext.CurrentContext.CancellationToken</c>, MSTest's <c>TestContext.CancellationToken</c>).
    /// </summary>
    /// <param name="options">Run options; each one left <see langword="null"/> comes from the config file's <c>testDefaults</c>, else its default.</param>
    /// <param name="body">The test body; each run receives its own <see cref="CrucibleContext"/>.</param>
    /// <param name="cancellationToken">Cancels the gate.</param>
    /// <returns>A task that completes when the gate passed.</returns>
    /// <exception cref="CrucibleVerdictException">The gate did not pass; the message is the failure report.</exception>
    /// <exception cref="CrucibleException">A usage or configuration error, as for <see cref="Gate(RunOptions, Func{CrucibleContext, Task})"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled before the gate ended.</exception>
    public static Task Gate(RunOptions options, Func<CrucibleContext, Task> body, CancellationToken cancellationToken) =>
        Engine.GateAsync(options, body, cancellationToken);

    internal static GateEngine Engine => LazyEngine.Value;

    /// <summary>Adds facts to the current run. See <see cref="CrucibleContext.Facts(string[])"/>.</summary>
    /// <param name="facts">The facts.</param>
    /// <exception cref="CrucibleException">A <see cref="CrucibleErrorKind.Usage"/> error for a null fact or outside a running gate body.</exception>
    public static void Facts(params string[] facts) => Context.Facts(facts);

    /// <summary>Adds facts to the current run. See <see cref="CrucibleContext.Facts(IEnumerable{string})"/>.</summary>
    /// <param name="facts">The facts.</param>
    /// <exception cref="CrucibleException">A <see cref="CrucibleErrorKind.Usage"/> error for a null fact or outside a running gate body.</exception>
    public static void Facts(IEnumerable<string> facts) => Context.Facts(facts);

    /// <summary>
    /// Asks the judge about the current run. See <see cref="CrucibleContext.CoherentAsync"/>.
    /// </summary>
    /// <param name="actual">The output of the system under test.</param>
    /// <param name="expectation">One natural-language claim about it.</param>
    /// <param name="options">Facts for this call only.</param>
    /// <param name="cancellationToken">Cancels the judge call.</param>
    /// <returns>The judge's verdict.</returns>
    /// <exception cref="CrucibleException">As <see cref="CrucibleContext.CoherentAsync"/>, and a <see cref="CrucibleErrorKind.Usage"/> error outside a running gate body.</exception>
    public static async Task<bool> CoherentAsync(
        string actual, string expectation, CoherentOptions? options = null, CancellationToken cancellationToken = default) =>
        await Context.CoherentAsync(actual, expectation, options, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asks the judge for a value, for the current run. See <see cref="CrucibleContext.ExtractAsync{T}"/>.
    /// </summary>
    /// <typeparam name="T"><see cref="string"/>, <c>int?</c>, <c>long?</c>, <c>double?</c> or <c>bool?</c>.</typeparam>
    /// <param name="text">The text to extract from.</param>
    /// <param name="description">What to extract, in natural language.</param>
    /// <param name="options">Strict mode and facts for this call only.</param>
    /// <param name="cancellationToken">Cancels the judge call.</param>
    /// <returns>The extracted value, or <see langword="null"/>.</returns>
    /// <exception cref="CrucibleException">As <see cref="CrucibleContext.ExtractAsync{T}"/>, and a <see cref="CrucibleErrorKind.Usage"/> error outside a running gate body.</exception>
    public static async Task<T?> ExtractAsync<T>(
        string text, string description, ExtractOptions? options = null, CancellationToken cancellationToken = default) =>
        await Context.ExtractAsync<T>(text, description, options, cancellationToken).ConfigureAwait(false);

    internal static void UseJudge(IJudgeClient client) => _judge = client;
}
