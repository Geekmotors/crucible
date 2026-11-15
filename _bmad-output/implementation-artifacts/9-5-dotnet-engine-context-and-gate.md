---
baseline_commit: a50415c
---

# Story 9.5: The engine, the ambient run context and the gate

Status: done

## Story

As a .NET developer testing an AI feature,
I want `await Crucible.Gate(options, async ctx => …)` to run my body N times concurrently, give each run its own facts and judge records through an ambient `Crucible.Facts/CoherentAsync/ExtractAsync<T>/Context` API, and fail with Crucible's report when the pass rate misses the threshold,
so that I can gate non-deterministic behaviour inside any test framework today, and the xUnit adapter (9.6) only has to call the same engine.

## Acceptance Criteria

1. **Public API** (namespace `CrucibleAI` — renamed from `Geekmotors.Crucible` in review; package id and assembly name stay `Geekmotors.Crucible`, XML docs on every public member):
   `static class Crucible` with `Task Gate(Func<CrucibleContext, Task> body)`, `Task Gate(RunOptions options, Func<CrucibleContext, Task> body)`, `CrucibleContext Context { get; }` (usage error outside a run), `void Facts(params string[] facts)`, `void Facts(IEnumerable<string> facts)`, `Task<bool> CoherentAsync(string actual, string expectation, CoherentOptions? options = null, CancellationToken cancellationToken = default)`, `Task<T?> ExtractAsync<T>(string text, string description, ExtractOptions? options = null, CancellationToken cancellationToken = default)`; `sealed class CrucibleContext` with the same three bound to its run (works where the ambient run is lost; `usage` once its run has settled) and a `Run` number; `sealed record CoherentOptions { Facts }`, `sealed record ExtractOptions { Strict, Facts }`; `sealed class CrucibleVerdictException : Exception` with `Outcome` (`VerdictOutcome`: `Failed`, `Errored`), `Message` = the report, `InnerException` = a representative run's cause. `UseJudge(IChatClient)` is 9.7: leave a seam (the engine takes any internal `IJudgeClient`), expose nothing.
2. **`ExtractAsync<T>` types**: `T` is `string`, `int?`, `long?`, `double?` or `bool?` (the judge may answer null, so value types are asked for in their nullable form); any other `T` (a non-nullable value type included) is a `usage` error when called, before any judge call. `int?`/`long?` send `TYPE number` and accept only a whole number in range (anything else is an unusable reply: re-asked once, then `infra` retryable).
3. **Engine** (internal, PORTING §2 steps 1–7): resolves options at run time (built-in → config `testDefaults` → group layers given by the caller → declared options); config loaded lazily once per process (9.3 `ConfigLoader`, real boundaries) with errors as `config`; starts `runs` runs at once (or one after another when the caller asks for sequential), each in its own `AsyncLocal` scope with a fresh `RunRecord` and a bound `CrucibleContext`; arms the test deadline on an injected `TimeProvider`, shared by every judge call; settles each run (returned → passed; first `CrucibleException` in the `InnerException` chain `infra` → errored; a `CrucibleVerdictException`, or a first `CrucibleException` `usage`/`config` → halt, cancel, rethrow the thrown exception unchanged; else failed); halts early on a fatal run (`infra`, not retryable) or the deadline, discarding unsettled runs, throwing `CrucibleVerdictException(Errored)` with the aborted/expired report; otherwise waits for every run, computes the verdict (9.2), throws with the report (9.4) unless passed; writes the passing report at `full`/`debug` and the debug trace (9.4) to an output that can never fail the test; closes every context.
4. **Adapter entry point** (internal, for 9.6): one call taking the group layers (outer → inner), the declared options, a per-run body factory (called once per run, with the run number, outside the run's ambient scope, so the adapter can build a fresh test-class instance per run and a constructor cannot reach the run) and a sequential flag.
5. **Deferred from 9.3/9.4**: a failed judge call is recorded as `ErroredCall` with its provider attempt count (counted through a provider trace hook); provider (`request`, `response`, `failure`) and exchange (`judge reply rejected`, `judge recorded`) trace events are emitted with the run tag; `ConfigLoader` keeps the raw `testDefaults` node for the trace header; the passing report is written at `full`/`debug`.
6. **Isolation tests** (red first, all green): concurrent runs of one gate see only their own facts; facts added after awaits, in `Task.Run` and in continuations stay in their run; a value set inside an awaited helper reaches the rest of the run; a call after the run settles, or outside any run, is `usage`; a background task outliving its run cannot write into a later run; two different gates running concurrently never see each other's facts; `ExecutionContext.SuppressFlow` / `ThreadPool.UnsafeQueueUserWorkItem` lose the ambient run (`usage`) while a `CrucibleContext` captured explicitly still works until its run settles.
7. **Engine tests**: runs start together; the deadline; fatal abort with discarded runs; rethrow of `usage`/`config` (directly and as an inner exception) unchanged; verdict exception outcome, report and cause; sequential runs; passing report at `full`/`debug` only; a broken output never fails the test; `ErroredCall` attempts; the trace header with the raw `testDefaults`.
8. **Surface check for C#**: a test maps every `spec/surface.json` item (functions, run options, call options, config fields, providers, environment variables, error kinds, verdict outcomes, report text) to the C# API, or to an explicit "not in C#" list with reasons; it fails when the manifest gains an unmapped item, and checks mapped C# members exist (public members by reflection, the rest against the lists the code keeps).
9. **Live smoke**: an opt-in test project `dotnet/tests/Geekmotors.Crucible.LiveTests` runs one `Crucible.Gate` (runs 1, threshold 1) with one `CoherentAsync` and one `ExtractAsync<int?>` against OpenRouter with a copy of `typescript/e2e/crucible.config.json` (≤ 2 judge calls), only when `CRUCIBLE_JUDGE_KEY` is set; `make dotnet-live` runs it; the default `dotnet test` and CI never run it. Not run in this story.
10. **Gates**: in `dotnet/` `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes`; TypeScript `npm test` and `make spec-goldens-check` untouched and green.

## Tasks / Subtasks

- [x] Story file, sprint status
- [x] Deferred plumbing, TDD (AC 5): `JudgeConfig.TestDefaultsNode` (raw); `IJudgeObserver` provider/exchange events; `IRetryBackoff` exposes the delay it waits; `ErroredCall` recorded with attempts; `JudgeRecordedEvent` for every recorded call
- [x] Extract target types, TDD (AC 2)
- [x] Public types (AC 1): `VerdictOutcome` (internal zone enum renamed `VerdictZone`), `CrucibleVerdictException`, `CoherentOptions`, `ExtractOptions`, `CrucibleContext`, `Crucible`
- [x] Engine, TDD (AC 3, 4, 7): `RunScope`, `RunState`, `RunClassifier`, `RunBatch`, `GateEngine`, `GateRequest`, `EngineSession`
- [x] Isolation tests (AC 6)
- [x] Surface check (AC 8)
- [x] Live smoke project + `make dotnet-live` (AC 9)
- [x] Gates (AC 10)

### Review Findings

- [x] [Review][Decision] Public static class `Crucible` clashed with namespace `Geekmotors.Crucible` (consumers in `Geekmotors.*` namespaces could not compile `Crucible.Gate`; CA1724) — resolved: root namespace renamed to `CrucibleAI` (sub-namespaces `CrucibleAI.Engine` etc., tests `CrucibleAI.Tests`); package id and assembly name stay `Geekmotors.Crucible`; `ConsumerNamespaceTests` compiles `Crucible.Gate` from `namespace Geekmotors.Consumer` (mutation-checked: re-adding a `Geekmotors.Crucible` namespace breaks it); README, dotnet/README, epics Epic 9 intro updated
- [x] [Review][Decision] No run cancellation token for the system under test — resolved: `CrucibleContext.CancellationToken` (the run's token, cancelled when the gate ends: all settled, halt, abort, deadline), test `TheTestDeadlineCancelsTheRunsToken`; a `CancellationToken` on `Gate` itself deferred to 9.6
- [x] [Review][Decision] Session `Lazy` caches a config failure for the process — resolved: keep
- [x] [Review][Decision] `ExtractAsync<T>` accepts any `T` at compile time — resolved: keep as is (documented)
- [x] [Review][Decision] Synchronous `GateRequest.BodyFor` (no async per-run setup, no per-run teardown) — resolved: do it in 9.6 (deferred-work)
- [x] [Review][Patch] Body returning a null `Task` counted as a failed run (NRE) — now a `usage` error that halts the gate [GateEngine.cs:83]
- [x] [Review][Patch] Runs closed after the halt token was cancelled — now closed first so no call can start between end and cancel [GateEngine.cs:47]
- [x] [Review][Patch] Missing judge key recorded as an `ErroredCall` with 0 attempts (TS reads the key outside the recorded region) — config errors no longer recorded [AssertionRun.cs:16]
- [x] [Review][Patch] No test proved in-flight judge calls are cancelled at the deadline — added `TheTestDeadlineCancelsTheJudgeCallsInFlight` (mutation-checked) [GateEngineTests.cs]
- [x] [Review][Patch] Wrapped `config` error rethrow untested — wrapper test is now a usage/config theory [GateEngineTests.cs]
- [x] [Review][Patch] First-failed-run cause test could not tell runs apart — distinct exceptions, run 1 settles last [GateEngineTests.cs]
- [x] [Review][Patch] Two-gates isolation test checked only one foreign fact — now every run of the other gate [IsolationProbe.cs]
- [x] [Review][Patch] Inline helpers in `RunIsolationTests` — moved to `Harnesses/IsolationProbe.cs`
- [x] [Review][Patch] Isolation gaps untested — added caller-has-no-run-after-gate, nested gate context, blocking on a gate from a stalled single-threaded `SynchronizationContext` [RunIsolationTests.cs]
- [x] [Review][Patch] XML docs: nested-gate verdict rethrow, null-task usage, synchronous usage throw [Crucible.cs]; long-lived workers keep the run they started in [CrucibleContext.cs]
- [x] [Review][Defer] A settled run's fire-and-forget judge calls are not cancelled until the batch ends and still trace/record into the settled run (TS parity) [RunState.cs] — deferred, design follow-up
- [x] [Review][Defer] Deadline firing before a parallel run's `Task.Run` starts still runs that body (results ignored) [GateEngine.cs:Start] — deferred, not deterministically testable
- [x] [Review][Defer] `long?` extraction goes through `double`: integers above 2^53 are rounded silently [ExtractTarget.cs] — deferred
- [x] [Review][Defer] Surface check: exception types not scanned; `reportText:`/`infra(retryable)` mappings true by construction [CSharpSurface.cs] — deferred
- [x] [Review][Defer] Trace shows no `Aborted` event when cancellation hits during backoff; `RunBatch.Settle` throwing would hang to the deadline (only via exotic `Exception.Message` getters); `AggregateException` chain walk follows only the first inner [ChatCompletionsClient.cs, RunBatch.cs, RunClassifier.cs] — deferred
- [x] [Review][Dismiss] `GateEngineTests` assert `AbortedMarker`/`ExpiredMarker` in `CrucibleVerdictException.Message` — accepted: the message IS the failure report (AC1), an observable output, not error message text

## Dev Notes

### Contract

- PORTING §2 *The engine* steps 1–7 and *Per-run context* (C# row): `AsyncLocal<T>` set inside each run's task before the body starts; flows into awaits, `Task.Run`, continuations, never back up; `SuppressFlow`/`UnsafeQueueUserWorkItem` lose it; the context object is the fallback.
- TS reference (behaviour only): `typescript/src/core/runner.ts` (classification, halt, discard, representative cause = first result with the verdict's status), `state.ts`/`run-handle.ts` (context closed when its own run settles, and every context at the end), `judge-exchange.ts` (`ErroredCall` on any exchange failure, attempts = provider attempts across both asks), `retry.ts` (trace order: request, response | failure with `retry`/`fatal`/`exhausted`/`aborted`), `trace-reporter.ts` (`reportPassed` writes `report + "\n"` when verbosity is not default).
- `describe(cause)` = `<name>: <message>`; C# name = the exception's type name (`GetType().Name`).
- The expired verdict exception's cause is an `infra` retryable `CrucibleException` ("did not finish within N ms"); the aborted one's is the fatal run's exception.

### C# design decisions

- `Crucible` (public static) holds only a lazily built production `GateEngine`; the ambient slot is one `AsyncLocal<RunState?>` behind `RunScope`, shared by every engine, so the global functions work inside any engine's runs (tests build engines from fakes and call the public API inside them).
- `CrucibleContext` holds its `RunState` explicitly; it never needs the ambient slot, which is why it survives `SuppressFlow`. Closed on settle → `usage` (`Facts` throws, the async calls return faulted tasks).
- Body factory: `Func<int, Func<CrucibleContext, Task>>` called inside the run's task but before the ambient slot is set (a constructor that calls `Crucible.Facts` gets `usage`); a throwing factory fails that run.
- Facts typed `IReadOnlyList<string>` / `params string[]` / `IEnumerable<string>`. Extension path for attachments: add a `Fact`/attachment type later as new overloads (`Facts(params Fact[])`) and a new options property (e.g. `Attachments`), both additive; `string` stays the exact match for existing calls, so no signature changes.
- Public `VerdictOutcome` has `Failed`/`Errored` only (TS `VerdictOutcome`); the internal three-zone enum becomes `VerdictZone`.

### Testing standards

TDD red-first; one type per file; no comments except public XML docs; outcomes not interactions; fakes one per file in `Fakes/`, harnesses in `Harnesses/`, constants in `Constants/`; assert error kind only; `ConfigureAwait(false)` in `src`; no non-ASCII in `.cs`; never touch real process state or files in unit tests (engine built from fakes; `Crucible.Gate` tested only where it fails before loading config).

### Previous story intelligence (9.2–9.4)

- 9.4: assertion services `CoherentAssertion.AskAsync(AssertionRun, CoherentCall)`, `ExtractAssertion.AskAsync(AssertionRun, ExtractCall)`; `RunRecord` thread-safe; `ReportRenderer(ReportSettings).Render(TestRecord)`; `DebugTrace(TraceSettings, ITraceOutput).Write(TraceEvent)`; `TraceSettings.TestDefaults` is a raw `JsonObject`; deferred: `ErroredCall`, trace hooks, passing report, raw `testDefaults`.
- 9.3: `JudgeExchange(IJudgeClient, TimeProvider)`, `ChatCompletionsClient(JudgeConfig)` production ctor; `ManualTimeProvider`, `ImmediateBackoff`, `ScriptedHandler`, `ProviderHarness`.
- 9.2: `RunOptionsValidator.Validate(options, RunOptionsSource.Test)`, `RunOptionsResolver.Resolve(layers)`, `VerdictCalculator.Compute`; MTP runner; lock files `--locked-mode`.

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro (isolation), Story 9.5, 9.6
- `spec/PORTING.md` §1 (public API, errors), §2 (engine, per-run context), §3; `spec/surface.json`
- `docs/typescript/api.md` (`gate`, `t`, `coherent`, `extract`, `facts`, errors, exported types)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- Red phases: plumbing tests compile-red on `JudgeConfig.TestDefaultsNode`; engine, isolation and public API tests compile-red on the missing `Geekmotors.Crucible.Engine` namespace (now `CrucibleAI.Engine`), `CrucibleContext`, `GateRequest`, `GateEngine`. Surface check verified to bite by deleting the `t.extract` mapping (2 failures per TFM).
- Mutation checks: replacing the `AsyncLocal` slot with one shared slot fails 11 isolation rows (concurrent runs, continuations, helper, two gates, SuppressFlow, UnsafeQueueUserWorkItem, background task); contexts that never close fail the settled-context and background-task tests.
- xUnit analyzers: `Timeout` tests must reference `TestContext.Current.CancellationToken` (xUnit1069): the rendezvous and awaited gates use it as the hang guard.
- `IsTestProject=false` does not keep an xunit.v3 project out of `dotnet test` (MTP still launches it; the property was dropped): the live smoke is `[Fact(Explicit = true)]` (reported "not run" by default, whatever the environment) and also skips without `CRUCIBLE_JUDGE_KEY`; `make dotnet-live` runs it with `-explicit only -failSkips`.
- Test suite run 5 times in a row: no flaky rows.

### Completion Notes List

- Public API (all XML-documented): `Crucible` (`Gate` x2, `Context`, `Facts` x2, `CoherentAsync`, `ExtractAsync<T>`), `CrucibleContext` (`Run`, `Facts` x2, `CoherentAsync`, `ExtractAsync<T>`), `CoherentOptions { Facts }`, `ExtractOptions { Strict, Facts }`, `CrucibleVerdictException { Outcome }`, `VerdictOutcome { Failed, Errored }`. The internal three-zone enum is now `Verdicts.VerdictZone`. No `UseJudge`: the seam is `EngineSession(JudgeConfig, IJudgeClient, TimeProvider, ITraceOutput)`.
- `ExtractAsync<T>`: `T` is `string`, `int?`, `long?`, `double?` or `bool?`; a non-nullable value type (`int`) or any other type is `usage` at call time, before the judge (it could not hold the judge's null). `int?`/`long?` send `TYPE number`; a fraction or out-of-range number is an unusable reply (re-asked, then `infra` retryable). Recorded extractions keep the double, so reports print numbers as before.
- Engine: `GateEngine.RunAsync(GateRequest(Groups, Declared, BodyFor, Sequential))` is the 9.6 entry point; `GateEngine.GateAsync(options, body)` validates (usage) and calls it with no groups. Session (config + judge client + renderer + trace) built lazily once per engine (`Lazy`, so a config error is cached for the process); `Crucible` holds one production engine. Each run: `Task.Run`, ambient cleared, `BodyFor(run)` called (outside the run), ambient set, body awaited, context closed, settled into `RunBatch` (lock + `TaskCompletionSource`), which ends on all settled, a halt (rethrow), a fatal run (aborted, unsettled discarded) or the `TimeProvider` deadline (expired, unsettled discarded). Then the shared cancellation is cancelled, every context closed, and the verdict/report/exception produced. In-flight bodies are not awaited after a halt (TS parity).
- Classification: first `CrucibleException` down `InnerException` (cycle-safe): infra retryable → errored, infra not retryable → fatal, usage/config → halt; `CrucibleVerdictException` itself → halt; anything else → failed. Halt rethrows the thrown exception unchanged (`ExceptionDispatchInfo`).
- Deferred items done: `ErroredCall(cause, attempts)` recorded for any exchange failure, attempts counted from `ProviderRequestEvent`s (both asks); provider request/response/failure (retry with the actual backoff delay, fatal, exhausted, aborted) and judge-reply-rejected/judge-recorded events reach the debug trace tagged `run #n`; `JudgeConfig.TestDefaultsNode` keeps the raw `testDefaults` (`{}` when absent) for the trace header; the passing report is written (`report + "\n"`) at `full`/`debug`, never failing the test. `IJudgeClient.CompleteAsync` and `JudgeExchange.AskAsync` take an `IJudgeObserver`; `IRetryBackoff` splits `Delay(retry)` from `WaitAsync(delay)`.
- Isolation tests (all green, net8.0 and net10.0): concurrent runs of one gate (10 runs meeting at a rendezvous) see only their own facts; facts after `await`, in `Task.Run`, in a continuation stay in their run; facts and `Crucible.Context` reached in an awaited helper reach the rest of the run; outside any run `Facts`/`Context`/`CoherentAsync`/`ExtractAsync` are usage; through a context after its run settled all three are usage; a background task outliving its run gets usage and its fact never reaches a later run; two gates via `Task.WhenAll` never see each other's facts; `SuppressFlow` and `UnsafeQueueUserWorkItem` lose the ambient run (usage) while the captured context still adds facts; the body factory runs outside the run (a `Crucible.Facts` in it is usage). Facts are observed through what the judge receives (a fake judge that extracts its own prompt).
- Engine tests: pass, runs start together, run numbers, sequential one at a time, factory once per run, testDefaults resolution, failed verdict with the first failed run's exception, threshold met despite failures, errored (direct and wrapped infra), usage/config rethrown unchanged (direct, wrapped, nested verdict), fatal abort (errored, fatal cause, aborted report), deadline (errored, infra retryable cause, expired report), config error, passing report at full/debug only, broken output never fails, trace header testDefaults as written, provider retry and judge-recorded lines tagged with the run, `ExtractAsync<int?>`, `ExtractAsync<int>` usage.
- Surface check (`Spec/SurfaceTests`): every `surface.json` item (operations, functions, run options, call options, config fields, providers, environment, error kinds, verdict outcomes, report text) maps to a C# member found by reflection / the lists the code keeps (`ConfigLoader.Fields`, `ConfigLoader.ProviderNames`, `ChatEndpoint.DefaultKeyVariable`, `ConfigLoader.VerbosityVariable`, `SpecOperations.Implemented`) or to a reason: `crucible.it` (xUnit adapter attributes, 9.6), `crucible.describe` (class/assembly attributes, 9.6), `crucible.load`/`append` (deprecated aliases). Every ported item with fixtures has at least one cited case that runs in C#; no mapping outlives its item; every public member is mapped or a known C# addition (`Crucible.Context`, `CrucibleContext.Run`); the exported types are exactly the known list. A new manifest item fails its theory row.
- Live smoke: `dotnet/tests/Geekmotors.Crucible.LiveTests` (in the solution: built and format-checked by CI, never run by it), `crucible.config.json` copied from `typescript/e2e/crucible.config.json`; one `Crucible.Gate` (runs 1, threshold 1) with one `CoherentAsync` and one `ExtractAsync<int?>` (2 judge calls). `make dotnet-live` (needs `CRUCIBLE_JUDGE_KEY`, checked by `e2e-key`) builds it and runs `dotnet run --framework net10.0 -- -explicit only -failSkips` from the project folder. Not run in this story.
- Facts extension path (attachments): add new overloads (`Facts(params Fact[])`) and new option properties (`Attachments`); `string` stays the exact match for existing calls, so nothing breaks.
- Gates: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` green (0 warnings; 3000 rows, 2924 pass, 76 skipped: the 74 spec ids plus the live smoke "not run" on both TFMs); TS `npm test` 1477 passed; `make spec-goldens-check` green.

### File List

- `_bmad-output/implementation-artifacts/9-5-dotnet-engine-context-and-gate.md` (new)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (modified)
- `Makefile` (modified: `dotnet-live`)
- `dotnet/Geekmotors.Crucible.slnx` (modified: live test project)
- `dotnet/src/Geekmotors.Crucible/`: `Crucible.cs`, `CrucibleContext.cs`, `CoherentOptions.cs`, `ExtractOptions.cs`, `CrucibleVerdictException.cs`, `VerdictOutcome.cs` (new)
- `dotnet/src/Geekmotors.Crucible/Engine/`: `BatchEnd`, `EngineSession`, `GateEngine`, `GateRequest`, `RunBatch`, `RunClassifier`, `RunObserver`, `RunScope`, `RunSettlement`, `RunState`, `SettledRun` (new)
- `dotnet/src/Geekmotors.Crucible/Assertions/AttemptCounter.cs`, `Judging/ExtractNumber.cs`, `Judging/ExtractTarget.cs`, `Providers/IJudgeObserver.cs` (new)
- `dotnet/src/Geekmotors.Crucible/`: `Assertions/{AssertionRun,CoherentAssertion,ExtractAssertion,ExtractTypes}.cs`, `Config/{ConfigLoader,JudgeConfig}.cs`, `Judging/{ExtractReplyReader,JudgeExchange}.cs`, `Providers/{ChatCompletionsClient,ExponentialBackoff,IJudgeClient,IRetryBackoff}.cs`, `Runs/RunCause.cs`, `Reports/ReportRenderer.cs`, `Tracing/DebugTrace.cs`, `Verdicts/{Verdict,VerdictCalculator}.cs` (modified); `Verdicts/VerdictOutcome.cs` renamed `Verdicts/VerdictZone.cs`
- `dotnet/tests/Geekmotors.Crucible.LiveTests/`: `Geekmotors.Crucible.LiveTests.csproj`, `LiveSmokeTests.cs`, `LiveTexts.cs`, `crucible.config.json`, `packages.lock.json` (new)
- Review: `dotnet/tests/Geekmotors.Crucible.Tests/`: `ConsumerNamespaceTests.cs`, `Fakes/StalledSynchronizationContext.cs`, `Harnesses/{BlockingCaller,IsolationProbe}.cs` (new); every `.cs` file and the three `.csproj` `RootNamespace` values (namespace rename to `CrucibleAI`)
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `CrucibleTests.cs`, `Engine/{GateEngineTests,RunIsolationTests}.cs`, `Spec/SurfaceTests.cs`, `Constants/{EngineRuns,EngineTexts,TestLimits}.cs`, `Fakes/{HangingJudgeClient,PromptJudgeClient,QuietJudgeObserver}.cs`, `Harnesses/{ConcurrencyGauge,CSharpSurface,EngineHarness,Rendezvous,SurfaceItem,SurfaceManifest,SurfaceMapping,ThrownCapture}.cs` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `Assertions/{CoherentAssertionTests,ExtractAssertionTests}.cs`, `Config/ConfigLoaderTests.cs`, `Constants/{AssertionTexts,ConfigTexts}.cs`, `Fakes/{EchoJudgeClient,EchoSystemJudgeClient,ImmediateBackoff}.cs`, `Harnesses/{AssertionHarness,CallArguments,JudgeReplyFixtureHarness,ProviderFixtureHarness,SpecFixtureCatalog,TraceFixtureHarness,VerdictFixtureHarness}.cs`, `Judging/JudgeExchangeTests.cs`, `Providers/{ChatCompletionsClientTests,ExponentialBackoffTests}.cs` (modified)

### Change Log

- 2026-11-15: C# engine, ambient run context and `Crucible.Gate`: public API, run loop (concurrency, deadline, early halt, discarded runs, verdict exception, report, passing report, trace), `ErroredCall` with attempts, provider/exchange trace events, raw `testDefaults`, isolation tests, C# surface check, opt-in live smoke (`make dotnet-live`).
