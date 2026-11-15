using CrucibleAI.Engine;
using CrucibleAI.Providers;
using Microsoft.Extensions.AI;

namespace CrucibleAI;

/// <summary>
/// Crucible's entry point: <see cref="Gate(RunOptions, Func{CrucibleContext, Task})"/> runs a test body many times and
/// passes when enough runs pass; the other members reach the run they are called from.
/// </summary>
public static class Crucible
{
    private static readonly RunScope Scope = new();
    private static readonly EngineHost Host = new();

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

    internal static GateEngine Engine => Host.Engine;

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

    /// <summary>
    /// Makes <paramref name="chatClient"/> the judge for every Crucible test in this process, instead of the provider in
    /// <c>crucible.config.json</c>. Call it once, before the first gate or Crucible test starts: in an xUnit assembly
    /// fixture, an NUnit <c>[SetUpFixture]</c>, an MSTest <c>[AssemblyInitialize]</c> method or a module initializer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// With a chat client the config file is optional. When present, its <c>testDefaults</c>, <c>verbosity</c> and
    /// <c>excerptChars</c> still apply; <c>provider</c>, <c>model</c>, <c>baseUrl</c>, <c>apiKeyEnv</c> and <c>meta</c>
    /// are ignored, with one warning on standard error.
    /// </para>
    /// <para>
    /// Each judge call sends a system message and a user message with a copy of <paramref name="options"/> (choose the
    /// model, temperature and so on there or in the client; Crucible adds no tools, passes your tools and response format
    /// through and clears <c>ConversationId</c>). An <see cref="HttpRequestException"/> without a status code or with status
    /// 408, 429 or 5xx, a <see cref="TimeoutException"/>, an <see cref="IOException"/>, a cancellation Crucible did not ask
    /// for, or an exception whose public <c>int Status</c> property (on it or an inner exception) is 408, 429 or 5xx
    /// (<c>ClientResultException</c>, <c>RequestFailedException</c>) is retried twice, then errors the run; any other exception aborts the
    /// test as a non-retryable <see cref="CrucibleErrorKind.Infra"/> error. A client that caches responses
    /// (<c>CachingChatClient</c>, e.g. <c>UseDistributedCache</c>) triggers a warning on standard error: cached verdicts
    /// hide the judge's own variance.
    /// </para>
    /// </remarks>
    /// <param name="chatClient">The judge, any <c>Microsoft.Extensions.AI</c> chat client.</param>
    /// <param name="options">Options sent with every judge call (copied per call), or <see langword="null"/> for the client's defaults.</param>
    /// <exception cref="CrucibleException">
    /// A <see cref="CrucibleErrorKind.Usage"/> error for a <see langword="null"/> client, after the first gate or Crucible
    /// test started, or when a different client or options instance was already chosen. The same instances again are a no-op.
    /// </exception>
    public static void UseJudge(IChatClient chatClient, ChatOptions? options = null) => Host.UseChatClient(chatClient, options);

    internal static void UseJudge(IJudgeClient client) => Host.UseJudge(client);
}
