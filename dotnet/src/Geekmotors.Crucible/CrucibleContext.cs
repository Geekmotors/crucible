using CrucibleAI.Assertions;
using CrucibleAI.Engine;

namespace CrucibleAI;

/// <summary>
/// One run of a gate: its facts and its judge calls. Each run receives its own context, and
/// <see cref="Crucible.Context"/> returns it anywhere the run's async flow reaches.
/// Hold it explicitly where that flow is lost (<see cref="ExecutionContext.SuppressFlow"/>,
/// <see cref="ThreadPool.UnsafeQueueUserWorkItem(WaitCallback, object?)"/>, some native callbacks), and where
/// it is kept too long: a worker first started inside a run (a timer, a channel consumer, a lazily started
/// background task) stays in that run's flow, so hand such workers the context of the run they serve.
/// Once its run has passed, failed, errored or been discarded, every call is a
/// <see cref="CrucibleErrorKind.Usage"/> error.
/// </summary>
public sealed class CrucibleContext
{
    private readonly RunState _run;

    internal CrucibleContext(RunState run)
    {
        _run = run;
    }

    /// <summary>The run's number, from 1, as the failure report names it.</summary>
    public int Run => _run.Number;

    /// <summary>
    /// Cancelled when the gate ends: its runs all settled, a run halted or aborted it, or its test deadline passed.
    /// Pass it to the system under test so work a run started stops once the gate no longer waits for it.
    /// </summary>
    public CancellationToken CancellationToken => _run.Cancellation;

    /// <summary>
    /// Adds facts to this run, in order. Every later judge call of the run sends them. Facts are copied and never
    /// removed, and no other run sees them.
    /// </summary>
    /// <param name="facts">The facts: the ground truth the system under test worked from, in any text format.</param>
    /// <exception cref="CrucibleException">A <see cref="CrucibleErrorKind.Usage"/> error for a null fact, or once the run has ended.</exception>
    public void Facts(params string[] facts) => _run.AddFacts(facts);

    /// <summary>
    /// Adds facts to this run, in order. Every later judge call of the run sends them. Facts are copied and never
    /// removed, and no other run sees them.
    /// </summary>
    /// <param name="facts">The facts: the ground truth the system under test worked from, in any text format.</param>
    /// <exception cref="CrucibleException">A <see cref="CrucibleErrorKind.Usage"/> error for a null fact, or once the run has ended.</exception>
    public void Facts(IEnumerable<string> facts) => _run.AddFacts(facts);

    /// <summary>
    /// Asks the judge whether <paramref name="actual"/> is consistent with the claim <paramref name="expectation"/>,
    /// given the run's facts as they stand now (and the call's own facts). Never throws on a false verdict.
    /// </summary>
    /// <param name="actual">The output of the system under test.</param>
    /// <param name="expectation">One natural-language claim about it.</param>
    /// <param name="options">Facts for this call only.</param>
    /// <param name="cancellationToken">Cancels the judge call.</param>
    /// <returns>The judge's verdict.</returns>
    /// <exception cref="CrucibleException">
    /// <see cref="CrucibleErrorKind.Usage"/> for invalid arguments or once the run has ended;
    /// <see cref="CrucibleErrorKind.Config"/> for a missing judge key; <see cref="CrucibleErrorKind.Infra"/> when the judge failed.
    /// </exception>
    public Task<bool> CoherentAsync(string actual, string expectation, CoherentOptions? options = null, CancellationToken cancellationToken = default) =>
        _run.CoherentAsync(new CoherentCall(actual, expectation, options?.Facts), cancellationToken);

    /// <summary>
    /// Asks the judge for the one value <paramref name="text"/> commits to for <paramref name="description"/>, or
    /// <see langword="null"/> when it commits to none.
    /// </summary>
    /// <typeparam name="T">
    /// <see cref="string"/>, <see cref="Nullable{T}"/> of <see cref="int"/>, <see cref="long"/> or <see cref="double"/>
    /// (sent as a number; <see cref="int"/> and <see cref="long"/> accept only a whole number in range), or
    /// <see cref="Nullable{T}"/> of <see cref="bool"/>. Value types are asked for in their nullable form
    /// (<c>ExtractAsync&lt;int?&gt;</c>) because the judge may answer null; any other type is a
    /// <see cref="CrucibleErrorKind.Usage"/> error.
    /// </typeparam>
    /// <param name="text">The text to extract from.</param>
    /// <param name="description">What to extract, in natural language.</param>
    /// <param name="options">Strict mode and facts for this call only.</param>
    /// <param name="cancellationToken">Cancels the judge call.</param>
    /// <returns>The extracted value, or <see langword="null"/>.</returns>
    /// <exception cref="CrucibleException">
    /// <see cref="CrucibleErrorKind.Usage"/> for invalid arguments, an unsupported <typeparamref name="T"/> or once the run has ended;
    /// <see cref="CrucibleErrorKind.Config"/> for a missing judge key; <see cref="CrucibleErrorKind.Infra"/> when the judge failed.
    /// </exception>
    public async Task<T?> ExtractAsync<T>(string text, string description, ExtractOptions? options = null, CancellationToken cancellationToken = default) =>
        (T?)await _run.ExtractAsync(new ExtractCall(text, description, typeof(T), options?.Strict, options?.Facts), cancellationToken)
            .ConfigureAwait(false);
}
