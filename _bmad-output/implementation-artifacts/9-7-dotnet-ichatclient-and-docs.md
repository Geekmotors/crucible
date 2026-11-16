---
baseline_commit: 8a8afc3
---

# Story 9.7: `IChatClient` judge, response-caching guard, docs

Status: done

## Story

As a .NET developer who already builds AI features on `Microsoft.Extensions.AI`,
I want to hand Crucible my own `IChatClient` as the judge (or keep the config-file providers), be warned when caching would hide variance, and read a complete NuGet page and C# API reference,
so that I can adopt Crucible without a second provider setup and without misreading what N runs measure.

## Acceptance Criteria

1. **Dependency**: `Geekmotors.Crucible` references `Microsoft.Extensions.AI.Abstractions` (central version in `Directory.Packages.props`; supports `net8.0` and `net10.0`; lock files updated). Version choice: the latest 10.x line (10.11.0) rather than 11.0.0 (released 2026-11-10): a library dependency is a floor for every consumer, and 10.x keeps .NET 8/10 consumers on the `Microsoft.Extensions.AI` line their other packages use.
2. **Public API**: `Crucible.UseJudge(IChatClient chatClient, ChatOptions? options = null)` in namespace `CrucibleAI`, with XML docs.
   - Call it once before the first gate (xUnit: an assembly fixture; NUnit `[SetUpFixture]`; MSTest `[AssemblyInitialize]`; or a module initializer).
   - A `null` client is a `usage` error.
   - Calling it after the shared engine was first used (any `Crucible.Gate` or `[CrucibleFact]`/`[CrucibleTheory]` test started), or a second time with a different client or options, is a `usage` error (fixes deferred-work 9.6 "`UseJudge` after the shared engine was first built is silently ignored"; the internal `UseJudge(IJudgeClient)` seam follows the same rules). Calling it again with the same client and options is a no-op.
3. **Config with a chat client**: `crucible.config.json`/`.jsonc` becomes optional. Absent: built-in defaults (`verbosity` default, `excerptChars` 500, no `testDefaults`; `CRUCIBLE_VERBOSE` still applies). Present: `testDefaults`, `verbosity`, `excerptChars` and `$schema` are read and validated as before; unknown and key-like fields stay `config` errors; `provider`, `model`, `baseUrl`, `apiKeyEnv` and `meta` are ignored (not validated) and their presence writes ONE stderr warning naming them. Without a chat client the config file is required exactly as before.
4. **Request mapping**: each judge ask sends two `ChatMessage`s (`ChatRole.System` = the judge system prompt, `ChatRole.User` = the payload) with a fresh clone of the given `ChatOptions` (or `null`); Crucible adds no tools, temperature or response format; `meta` does not apply (model, temperature etc. come from the client or `options`). The reply is `ChatResponse.Text`, fed through the existing judge-exchange rules (strict JSON, one re-ask, `inferenceTimeout` via the `CancellationToken`).
5. **Error classification** (one attempt plus up to two retries with the providers' backoff for retryable failures):
   - cancellation of Crucible's token (gate end, deadline) → rethrown as cancellation (the exchange turns a deadline into `infra` retryable as today);
   - `HttpRequestException` without a status code or with 408, 429 or 5xx, `TimeoutException`, `IOException`, an `OperationCanceledException` not caused by Crucible's token (e.g. `HttpClient.Timeout`) → retryable; after the last attempt `infra` with `Retryable = true` (the run is errored);
   - anything else (other 4xx, provider SDK exceptions, `InvalidOperationException`, …) → `infra` with `Retryable = false` immediately (the test aborts).
   - debug trace shows provider request/response/failure lines with provider `"chat-client"` and model = `options.ModelId`, else the client's `ChatClientMetadata.DefaultModelId`, else `"(client default)"`.
6. **Caching guard**: when the client passed to `UseJudge` is, or wraps (found through `IChatClient.GetService`), a `Microsoft.Extensions.AI.CachingChatClient` (e.g. `DistributedCachingChatClient`), Crucible writes ONE stderr warning (judge caching replays verdicts and hides judge variance); not fatal. Detection resolves the type by name so `Geekmotors.Crucible` does not depend on `Microsoft.Extensions.AI`. The system under test's client is invisible to Crucible: no API, the docs warn instead (decision: a `WarnIfCaching` helper adds public surface for something one sentence of docs covers).
7. **Tests** (TDD red-first; fakes one per file; outcomes only): engine host rules (null, after start, twice different, same twice OK, internal seam), config-optional loading (absent file, ignored provider fields warn once, unknown field still `config`, `testDefaults` applied), chat-client judge (reply returned, retry then success, retryable exhausted, fatal, cancellation, options cloned per call), caching detection with the real `DistributedCachingChatClient` (test-only `Microsoft.Extensions.AI` + `Microsoft.Extensions.Caching.Memory`), a gate passing end-to-end through a fake `IChatClient` with no config file.
8. **Docs**: `dotnet/README.md` is the NuGet page (install, quick start, config + key env + copy behaviour, attributes and resolution, facts/coherent/extract with types, strict, runs/threshold and the pass-rate warning, the gate for NUnit/MSTest, `UseJudge(IChatClient)`, caching warning, failure report example, links); new `docs/dotnet/api.md` (full C# API reference mirroring `docs/typescript/api.md`); root `README.md` C# row accurate for the moment of merge (0.1.0 published in 9.8); shared concept docs get light C# snippets where TS-only examples would confuse; `spec/PORTING.md` §4 records the C# port (status, spec commit, skips).
9. **Gates**: in `dotnet/`: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes`; in `typescript/`: `npm run lint && npm run format:check && npm test`; root `make spec-goldens-check`.

## Tasks / Subtasks

- [x] Story file, sprint status
- [x] Dependency + lock files (AC 1)
- [x] Config: optional loading for a chat client (AC 3), TDD
- [x] `ChatClientJudge` (AC 4, 5), TDD
- [x] Caching probe (AC 6), TDD
- [x] `EngineHost` + `Crucible.UseJudge(IChatClient, ChatOptions?)` (AC 2), TDD; fixes the 9.6 deferred item
- [x] Docs (AC 8)
- [x] Surface manifest reasons if needed; gates (AC 9)

### Review Findings

- [x] [Review][Decision] Provider SDK exceptions are never retryable — resolved (b): the `InnerException` chain is walked for a public `int Status`; 408/429/5xx transient, other statuses fatal; tested with `ClientResultException`/`RequestFailedException`-shaped fakes; documented.
- [x] [Review][Decision] `ChatOptions.ConversationId` passthrough — resolved: cleared on the per-call copy (tested); tools/response format documented as the caller's choice.
- [x] [Review][Decision] Status-less `HttpRequestException` (DNS/TLS) and 501/505 retried — resolved: keep for parity with the provider path; deferred.
- [x] [Review][Patch] Caching probe ran after the judge was committed; a client whose `GetService` throws left the judge registered while `UseJudge` threw [dotnet/src/Geekmotors.Crucible/Engine/EngineHost.cs]
- [x] [Review][Patch] Blank `ChatOptions.ModelId` used as the trace model; now falls back to metadata, then `(client default)`; trace model labels (AC 5) now tested [dotnet/src/Geekmotors.Crucible/ChatClients/ChatClientJudge.cs]
- [x] [Review][Patch] Flaky `RunLifecycleTests.ABodyStillRunningAfterAHaltIsTornDownWhenItFinishes` (from 9.6): run 1 could halt before run 2's body started; run 1 now waits for run 2 to start [dotnet/tests/Geekmotors.Crucible.Tests/Engine/RunLifecycleTests.cs]
- [x] [Review][Patch] Docs: "same client and options" means the same instances; judge is per process (xUnit v3: per test project); Crucible never disposes the client; options must not change after the call; README sample dropped `Temperature = 0` (GPT-5 models reject it → fatal 400); README missing-file note qualified for `UseJudge`; README rules list null client; api.md `Errored` wording judge-neutral [dotnet/README.md, docs/dotnet/api.md, Crucible.cs XML docs]
- [x] [Review][Defer] `GateEngine`'s `Lazy<EngineSession>` caches a session-creation failure for the process — deferred, pre-existing
- [x] [Review][Defer] Retry backoff uses `TimeProvider.System`, not the host's injected clock (same as the provider client) — deferred, pre-existing pattern
- [x] [Review][Defer] Caching probe resolves the type in the default load context only and misses non-`CachingChatClient` caches — deferred, documented limit
- [x] [Review][Defer] No test against `Microsoft.Extensions.AI` 11.x consumers; net8.0 consumers get System.Text.Json 10.0.x transitively — deferred
- [x] [Review][Defer] NuGet README links point at `main` and says "until it is published"; PORTING embeds a commit hash — deferred to release prep
- [x] [Review][Defer] Internal `UseJudge(IJudgeClient)` seam tested for the after-start rule only; no test for `CRUCIBLE_VERBOSE` without a config file — deferred

## Dev Notes

### Current state (read before changing)

- `Crucible` (static): `Lazy<GateEngine>` built from `_judge` (internal `UseJudge(IJudgeClient)` just assigns `_judge`; after the lazy engine is created the assignment is silently ignored). `Crucible.Engine` is used by the xUnit adapter (`CrucibleTestRunner`). Keep `Crucible.Engine` internal and its behaviour.
- `GateEngine(Func<EngineSession>, TimeProvider)`: the session is created lazily on the first `RunAsync` (config is loaded then). `GateEngine(IJudgeClient?)` + `EngineSession.FromConfigFile` are the production wiring to replace with an `EngineHost`.
- `EngineSession(JudgeConfig, IJudgeClient, TimeProvider, ITraceOutput)` uses `JudgeConfig` for verbosity, test defaults, excerpt chars and the trace header (provider name via `ChatEndpoint.For(config).Name`, model, meta).
- `ConfigLoader.Load()` requires a file and `provider`/`model`; `ReadFile()` holds the missing-file message (keep it unchanged for the provider path).
- `ChatCompletionsClient` is the provider transport: 3 attempts, `IRetryBackoff`, trace events `ProviderRequestEvent`/`ProviderResponseEvent`/`ProviderFailureEvent`, fatal → `infra(false)`, exhausted → `infra(true)`. Mirror its shape for `ChatClientJudge`.
- `CrucibleException.Retryable = false` aborts the whole test: classify unknown client exceptions as fatal deliberately (conservative: no silent retries of a programming error), document it.
- Fixture project installs its fake judge through `Crucible.UseJudge(IJudgeClient)` in an assembly fixture: that path keeps requiring the config file (it proves the config-copy targets).

### Previous story intelligence (9.5, 9.6)

- One type per file; fakes in `Fakes/`, harnesses in `Harnesses/`, constants in `Constants/`; assert error kind only; outcomes not interactions; `ConfigureAwait(false)` in `src`; restore with `--locked-mode`; xUnit1051 wants `TestContext.Current.CancellationToken`.
- Public `Crucible.Gate(..., CancellationToken)` overloads exist (9.6 review).
- Deferred to here: `UseJudge` after first engine build silently ignored.

### Testing standards

TDD red-first. The static `Crucible` is process state: test the `EngineHost` instance (injected config locator over a fake file system, warning sink, trace output, clock) rather than the static facade. Never touch real env/cwd or write files.

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro, Story 9.7
- `_bmad-output/implementation-artifacts/deferred-work.md` (9.6 items)
- `_bmad-output/implementation-artifacts/9-5-dotnet-engine-context-and-gate.md`, `9-6-dotnet-xunit-adapter.md`
- `spec/PORTING.md` §4 (record ports), `docs/typescript/api.md` (structure to mirror)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- Red phases: `ConfigLoaderTests` (compile-red on `LoadRunSettings`), `ChatClientJudgeTests` (compile-red on `ChatClientJudge`), `EngineHostTests` (compile-red on `EngineHost`). `ChatClientJudge` was written right after its tests, so it was mutation-checked: no per-call `Clone()` fails `EachCallGetsTheOptionsAsGiven`; 5xx not transient fails 3 tests. The caching test passing proves the by-name `CachingChatClient` lookup resolves against the real `Microsoft.Extensions.AI` 10.11.0.
- `Microsoft.Extensions.Options.Options` clashes with the test namespace `CrucibleAI.Tests.Options`: aliased in the harness.
- Full .NET suite run twice: no flaky rows.

### Completion Notes List

- **Dependency**: `Microsoft.Extensions.AI.Abstractions` 10.11.0 (net8.0 + net10.0; deliberate floor below 11.0.0, released 5 days ago). Test-only: `Microsoft.Extensions.AI` 10.11.0, `Microsoft.Extensions.Caching.Memory` 10.0.13 (real `DistributedCachingChatClient` for the caching test). All lock files updated.
- **API**: `Crucible.UseJudge(IChatClient chatClient, ChatOptions? options = null)` (XML docs). Rules live in the new internal `EngineHost` (the static `Crucible` holds one): null client → `usage`; after the shared engine was first used (`Crucible.Engine`, i.e. any gate or `[CrucibleFact]` test) → `usage`; a second, different client/options pair → `usage`; same pair again → no-op. The internal `UseJudge(IJudgeClient)` seam follows the same rules (deferred-work 9.6 item resolved) and still requires the config file.
- **Config with a chat client** (`ConfigLoader.LoadRunSettings`): file optional; when present `testDefaults`/`verbosity`/`excerptChars` apply, unknown and key-like fields stay `config` errors, `provider`/`model`/`baseUrl`/`apiKeyEnv`/`meta` are ignored (not validated) with one stderr warning. The missing-file `config` message now also mentions `Crucible.UseJudge`.
- **Mapping** (`ChatClients/ChatClientJudge`): system + user `ChatMessage`, `options?.Clone()` per call, `ChatResponse.Text` into the unchanged `JudgeExchange` (strict JSON, re-ask, inference deadline via the token). 3 attempts with the providers' backoff; transient = `HttpRequestException` without status or 408/429/5xx, `TimeoutException`, `IOException`, `OperationCanceledException` not from Crucible's token; exhausted → `infra(retryable)`; anything else → `infra(not retryable)` at once (aborts the test). Trace: provider `"chat-client"`, model = `options.ModelId` → `ChatClientMetadata.DefaultModelId` → `(client default)`.
- **Caching guard** (`ChatClients/CachingClientProbe`): resolves `Microsoft.Extensions.AI.CachingChatClient` by name (no dependency on the full package) and asks `client.GetService(type)`, which walks delegating pipelines; one stderr warning when a judge caches. SUT caching: docs only (no `WarnIfCaching` API: Crucible cannot see that client; a helper would add public surface for one sentence of docs).
- **Engine refactor**: `EngineSession` takes `RunSettings` + provider/model/meta labels (the `JudgeConfig` constructor delegates); `GateEngine(IJudgeClient?)` and `EngineSession.FromConfigFile` removed (replaced by `EngineHost`).
- **Docs**: `dotnet/README.md` rewritten as the NuGet page (absolute GitHub links); new `docs/dotnet/api.md`; root `README.md` (C# section, language table, layout); `docs/README.md` index row; light C# notes in `getting-started.md`, `configuration.md`, `runs-and-threshold.md`, `gotchas.md` (new "Response caching turns N runs into one sample" gotcha); `spec/PORTING.md` §4 "Ports" table (TS + C#: status, spec commit, 37 skips with reasons).
- **Surface check**: `Crucible.UseJudge` listed as a C# addition.
- **Gates**: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` green (0 warnings; 3150 passed, 80 skipped as before); TS `npm run lint`, `npm run format:check`, `npm test` (1477 passed); `make spec-goldens-check` green.
- **Not done (optional)**: no live `IChatClient` test (would need `Microsoft.Extensions.AI.OpenAI` as a test-only dependency); the machine-readable spec-version file is left to 9.8 packaging.

### File List

- `_bmad-output/implementation-artifacts/9-7-dotnet-ichatclient-and-docs.md` (new); `sprint-status.yaml`, `deferred-work.md` (modified)
- `README.md`, `docs/README.md`, `docs/configuration.md`, `docs/getting-started.md`, `docs/gotchas.md`, `docs/runs-and-threshold.md`, `spec/PORTING.md`, `dotnet/README.md` (modified); `docs/dotnet/api.md` (new)
- `dotnet/Directory.Packages.props`, all `packages.lock.json` files under `dotnet/` (modified)
- `dotnet/src/Geekmotors.Crucible/`: `Geekmotors.Crucible.csproj`, `Crucible.cs`, `Config/ConfigLoader.cs`, `Config/JudgeConfig.cs`, `Engine/EngineSession.cs`, `Engine/GateEngine.cs` (modified); `ChatClients/ChatClientJudge.cs`, `ChatClients/CachingClientProbe.cs`, `Config/RunSettings.cs`, `Engine/EngineHost.cs` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `Geekmotors.Crucible.Tests.csproj`, `Config/ConfigLoaderTests.cs`, `Constants/ConfigTexts.cs`, `Constants/TestLimits.cs`, `Harnesses/CSharpSurface.cs` (modified); `ChatClients/ChatClientJudgeTests.cs`, `Engine/EngineHostTests.cs`, `Constants/ChatClientFailure.cs`, `Constants/ChatClientTexts.cs`, `Fakes/HangingChatClient.cs`, `Fakes/OptionsMutatingChatClient.cs`, `Fakes/ScriptedChatClient.cs`, `Harnesses/ChatClientFailures.cs`, `Harnesses/EngineHostHarness.cs` (new)

### Change Log

- 2026-11-15: `Crucible.UseJudge(IChatClient, ChatOptions?)` with once-only rules (fixes silent late `UseJudge`), optional config with a chat client, chat-client judge with retry classification, judge caching warning, NuGet README, C# API reference, shared-doc C# notes, PORTING ports table.
