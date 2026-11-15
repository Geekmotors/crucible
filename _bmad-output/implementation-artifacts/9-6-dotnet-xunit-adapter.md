---
baseline_commit: 5d93be0
---

# Story 9.6: xUnit v3 adapter — `[CrucibleFact]`, `[CrucibleTheory]`, `[Crucible]`

Status: done

## Story

As a .NET developer testing an AI feature with xUnit v3,
I want to write `[CrucibleFact(Runs = 20, Threshold = 0.9)]` (or `[CrucibleTheory]` with my data rows) on an ordinary test method, with class or assembly `[Crucible(...)]` defaults, and reach the run through `Crucible.Facts/CoherentAsync/ExtractAsync<T>/Context`,
so that each test runs N times on its own class instances, reports ONE xUnit result with Crucible's verdict and failure report, and never shares facts with any other test, row, run or collection.

## Acceptance Criteria

1. **Package and project**: `dotnet/src/Geekmotors.Crucible.Xunit` (package id / assembly `Geekmotors.Crucible.Xunit`, multi-target `net8.0;net10.0`), depending on `Geekmotors.Crucible` and `xunit.v3.extensibility.core` 4.0.3 (the extensibility package, not `xunit.v3`, so it is not itself a test project). The three public attributes live in namespace `CrucibleAI` (one `using CrucibleAI;` serves the engine and the attributes); xUnit plumbing lives in `CrucibleAI.Xunit` (internal or `EditorBrowsable(Never)` where xUnit requires public types). XML docs on every public member. The engine gains no public API.
2. **Attributes**:
   - `CrucibleFactAttribute : FactAttribute` and `CrucibleTheoryAttribute : TheoryAttribute` (method), `CrucibleAttribute` (class and assembly), each with `Runs` (int), `Threshold` (double), `InferenceTimeout` (int ms), `TestTimeout` (int ms), `Strict` (bool), `Sequential` (bool). A property not set in the attribute usage is not declared (inherits from the next layer out); a setter records that it was set (attributes cannot take nullable value types). Invalid values (e.g. `Runs = 0`) are a `usage` error naming the attribute layer, failing that test.
   - Resolution: built-in → config `testDefaults` → assembly `[Crucible]` → class `[Crucible]` (outermost declaring class of nested classes first, then inner; inherited class attributes from base classes count, nearest wins) → method attribute. `Sequential` resolves the same way (default `false`).
   - xUnit's own `Skip`, `SkipUnless`/`SkipWhen`, `Explicit`, `DisplayName`, `Timeout`, `SkipExceptions` and traits keep their xUnit meaning. A skipped test never runs its body (no judge calls).
3. **Execution**: a `[CrucibleFact]`/`[CrucibleTheory]` test runs through `GateEngine.RunAsync` (shared with `Crucible.Gate`, one config load and one session per process): `runs` runs concurrently (`Sequential = true`: one after another), each run creating its OWN test-class instance (constructor with the class/collection/assembly fixtures and `ITestOutputHelper` xUnit resolved, then `IAsyncLifetime.InitializeAsync`) OUTSIDE the run scope, invoking the method (with the row's arguments for a theory) INSIDE the run, then disposing the instance (`IAsyncLifetime.DisposeAsync`, then `IAsyncDisposable.DisposeAsync` or `IDisposable.Dispose`) after the body finishes, outside the run. Async (`Task`, `ValueTask`, `ValueTask<T>`, `Task<T>`) and synchronous methods work; `async void` is refused like xUnit does. A failing constructor/initialise/dispose fails that run (its exception classified like a body exception). Static test methods run without an instance.
4. **Results**: ONE xUnit result per `[CrucibleFact]` and per theory data row (pre-enumerated rows and delay-enumerated rows alike): passed when the gate passes; failed with the `CrucibleVerdictException` (its message = the Crucible failure report) otherwise; a `usage`/`config` error rethrown unchanged fails the test with that exception.
5. **Cancellation and timeouts**: the gate is tied to xUnit's test cancellation (`TestContext.Current.CancellationToken`): cancelling ends the gate (runs' `CrucibleContext.CancellationToken` and in-flight judge calls cancelled) and the test reports the cancellation. The gate's `testTimeout` governs the runs; an xUnit `Timeout` on the attribute still applies to the whole test (documented: set it above `testTimeout` or leave it unset).
6. **Engine changes (internal)**: `GateRequest` takes an async per-run body factory returning the body plus a per-run teardown, and a `CancellationToken`; the factory runs outside the run scope (a constructor or `InitializeAsync` calling `Crucible.Facts` is `usage`); teardown runs after that run's body finishes, outside the run (also for bodies still running after a halt/deadline: they are disposed when they finish, possibly after the test result — documented). `Crucible.Gate` keeps its public signature. `InternalsVisibleTo` grants `Geekmotors.Crucible.Xunit`.
7. **Isolation tests in real xUnit** (red first): a fixture test project `dotnet/tests/Geekmotors.Crucible.Xunit.Fixture` (xUnit v3 native console runner, never run by `dotnet test` itself) is executed as a child process by `Geekmotors.Crucible.Xunit.Tests` with the JSON reporter; its judge is an in-process fake installed through an internal seam (no network), its config file found through the shipped config-copy targets. The tests prove: two Crucible tests in the same class, in different classes running in parallel, and in different collections running in parallel never see each other's facts; each theory row has its own facts; each of the N runs of one test has its own facts and its own class instance; facts never survive from one test to the next; `Crucible.Facts` in a plain `[Fact]`, a constructor, `IAsyncLifetime.InitializeAsync`, a class fixture, a collection fixture or a static initialiser is a `usage` error; one result per test and per theory row; the failure report is the xUnit failure message; `Sequential` runs one at a time; class/assembly `[Crucible]` defaults apply and the method overrides them; a skipped Crucible test does not run.
8. **Config discovery**: the package ships `build/Geekmotors.Crucible.Xunit.targets` (packed under `build/` and `buildTransitive/`) that copies `crucible.config.json` / `crucible.config.jsonc` from `$(MSBuildProjectDirectory)` to the output folder (`PreserveNewest`) without the user editing the project file; the repository's own test projects import it to dogfood it. The missing-config `config` error names the folder searched and says the test project's config is copied to the output folder by `Geekmotors.Crucible.Xunit` (place it beside the `.csproj`).
9. **Live smoke**: `dotnet/tests/Geekmotors.Crucible.LiveTests` gains one `[CrucibleFact(Runs = 1, Threshold = 1, Explicit = true)]` and one `[CrucibleTheory(Runs = 1, Threshold = 1, Explicit = true)]` with one row, each making one judge call (the live project stays at ≤ 4 judge calls total); `make dotnet-live` runs them; default `dotnet test` and CI never do. Not run in this story.
10. **Docs and CI**: `dotnet/README.md` (draft; final in 9.7) shows the attributes, resolution order, instance lifecycle, config copy, timeouts; the solution includes the new projects so CI builds, tests and format-checks them.
11. **Gates**: in `dotnet/`: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` (tests 3x for flakes); TypeScript `npm test` and `make spec-goldens-check` untouched and green.

## Tasks / Subtasks

- [x] Story file, sprint status
- [x] Engine (AC 6), TDD: async factory + teardown + cancellation in `GateRequest`; engine tests (teardown after body, outside the run; factory failure fails the run; cancellation ends the gate; teardown after a halt waits for the body)
- [x] Judge seam for the fixture (`Crucible.UseJudge(IJudgeClient)` internal, the 9.7 `UseJudge(IChatClient)` groundwork), TDD
- [x] Config-missing message (AC 8), TDD
- [x] Xunit project: attributes + settings resolution (AC 2), TDD in `Geekmotors.Crucible.Xunit.Tests`
- [x] Discoverers, self-executing test cases, test-case runner and gate test runner (AC 3–5)
- [x] Fixture project + child-process harness + isolation tests (AC 7), red first
- [x] Config-copy targets + packing (AC 8), dogfooded by the fixture and live projects
- [x] Live smoke additions (AC 9)
- [x] README, solution, surface-check reasons (AC 10)
- [x] Gates (AC 11)

## Dev Notes

### xUnit v3 (4.0.3) extension points — verified against the source at commit 384aa7f

- `FactAttribute` / `TheoryAttribute` are unsealed; the discoverer is found through `[XunitTestCaseDiscoverer(typeof(...))]` on the attribute type. `FactDiscoverer.CreateTestCase` and `TheoryDiscoverer.CreateTestCasesForDataRow` / `CreateTestCasesForTheory` are `protected virtual`: override them to return Crucible test cases (the theory discoverer keeps xUnit's pre-enumeration, skip and no-data rules).
- `ISelfExecutingXunitTestCase.Run(explicitOption, messageBus, constructorArguments, aggregator, cts, parallelMode, scheduler, methodFixtureMappings)` lets a test case run itself (`XunitTestMethodRunnerBaseContext.RunTestCase` checks it first). Crucible test cases subclass `XunitTestCase` and `XunitDelayEnumeratedTheoryTestCase` (both serializable; keep the `[Obsolete]` parameterless constructor for the deserializer) and run `CreateTests()` (one test, or one per delay-enumerated row) through a Crucible test-case runner (`XunitTestCaseRunnerBase<…>` with a context whose `RunTest(test)` calls the Crucible test runner).
- The Crucible test runner derives `XunitTestRunnerBase<XunitTestRunnerContext, IXunitTest>`: `IsTestClassCreatable` → false (no shared instance), `InvokeTest` → the gate. Skip, explicit, before/after attributes, xUnit `Timeout` and the message flow (`TestStarting`, `TestPassed`/`TestFailed`/`TestSkipped`, `TestFinished`) stay xUnit's.
- Per-run instance: same logic as `XunitTestRunnerBase.CreateTestClassInstance` (single public ctor, `ctxt.ConstructorArguments`, `Func<T>` unwrapping, `TypeActivator.Current.CreateInstance`), then `IAsyncLifetime.InitializeAsync`; dispose like `TestRunner.DisposeTestClassInstance` plus `IAsyncLifetime.DisposeAsync`.
- `TestContext.Current` is async-local: it flows into the engine's `Task.Run` runs, so `ITestOutputHelper` and `TestContext.Current.CancellationToken` work inside runs.

### Previous story intelligence (9.2–9.5)

- 9.5: `GateEngine.RunAsync(GateRequest(Groups, Declared, BodyFor, Sequential))` is the adapter entry point; `Crucible` holds one lazily built production engine (`EngineSession.FromConfigFile`); `RunScope` is one shared `AsyncLocal<RunState?>`; the factory is called inside the run's task before the ambient slot is set; bodies are not awaited after a halt; `CrucibleContext.CancellationToken` is cancelled when the gate ends. Deferred to 9.6: async factory + per-run teardown; a gate cancellation token wired to xUnit.
- 9.5 debug log: `IsTestProject=false` did not keep an xunit.v3 project out of `dotnet test`; xUnit analyzer xUnit1051/1069 wants `TestContext.Current.CancellationToken` in async tests.
- 9.3: `ConfigFileLocator` uses `Directory.GetCurrentDirectory()`; missing config message comes from `ConfigLoader.ReadFile`.
- Conventions: one type per file; fakes in `Fakes/`, harnesses in `Harnesses/`, constants in `Constants/`; assert error kind only; outcomes, not interactions; no comments except public XML docs; `ConfigureAwait(false)` in `src`; lock files with `--locked-mode`.

### Testing standards

TDD red-first; the adapter's behaviour is proven through real xUnit runs of the fixture project (the observable outcome: each fixture test's xUnit result); pure resolution logic is tested directly. Never touch real process state or write files in tests (the child process runs with its own working directory; nothing is written).

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro (isolation is first-class), Story 9.6
- `spec/PORTING.md` §2 *The runner adapter* (variants, group settings, xUnit collections)
- `_bmad-output/implementation-artifacts/deferred-work.md` (9.5 → 9.6 items)
- `_bmad-output/implementation-artifacts/9-5-dotnet-engine-context-and-gate.md`

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- Red phases: engine lifecycle tests (`RunLifecycleTests`) and the updated engine tests compile-red on `RunBody` / the 5-argument `GateRequest`; resolver tests compile-red on `CrucibleAttribute`, `CrucibleSettingsResolver`, `ICrucibleSettings`.
- The adapter's isolation tests were written against a working adapter, so each was mutation-checked instead (all bite): one shared test-class instance per test fails `EachRunHasItsOwnFactsAndItsOwnClassInstance`; calling the body factory inside the run fails the constructor, static-initialiser, `InitializeAsync` rows (4 failures); tearing down inside the run fails the lifetime rows (2); ignoring `Sequential` fails `SequentialRunsExecuteOneAtATime`; ignoring class/assembly layers fails both default rows; dropping the xUnit cancellation token fails `XunitsTestCancellationCancelsTheRuns`.
- The first cancellation fixture disposed its token registration when its body ended on the xUnit token, before the engine cancelled the run token (a test race, not an adapter bug): the body now waits only on the run token and keeps the registration.
- `IsTestingPlatformApplication=false` (with `UseMicrosoftTestingPlatformRunner=false`) keeps the fixture project out of `dotnet test` (9.5 found `IsTestProject=false` alone does not).
- `[assembly: Crucible(...)]` outside a `CrucibleAI.*` namespace needs `using CrucibleAI;` (attribute-name lookup picks `CrucibleAttribute` over the static class `Crucible`, which is not an attribute).
- Full .NET suite run 3 times in a row: no flaky rows.

### Completion Notes List

- **Projects**: `src/Geekmotors.Crucible.Xunit` (net8.0/net10.0, `xunit.v3.extensibility.core` 4.0.3, attributes in `CrucibleAI`, plumbing internal in `CrucibleAI.Xunit`), `tests/Geekmotors.Crucible.Xunit.Tests` (resolver unit tests + isolation tests over a child-process run), `tests/Geekmotors.Crucible.Xunit.Fixture` (real xUnit tests with a fake judge; xUnit native runner; never run by `dotnet test`). All in the solution, so CI builds, tests and format-checks them.
- **Attributes**: `CrucibleFactAttribute : FactAttribute`, `CrucibleTheoryAttribute : TheoryAttribute`, `CrucibleAttribute` (assembly/class, `Inherited = true`), each with `Runs`, `Threshold`, `InferenceTimeout`, `TestTimeout`, `Strict`, `Sequential`. A setter records the value in a private `CrucibleSettings` (nullable fields); unset reads 0/false and is inherited. Resolution: built-in, `testDefaults`, assembly, classes (outermost declaring class first; base before derived), method. Each layer is validated by the engine's `RunOptionsValidator` (`usage`, failing the test).
- **xUnit v3 hooks**: `[XunitTestCaseDiscoverer]` on the attributes; `CrucibleFactDiscoverer : FactDiscoverer` and `CrucibleTheoryDiscoverer : TheoryDiscoverer` return `CrucibleTestCase : XunitTestCase` (facts, pre-enumerated rows) and `CrucibleDelayEnumeratedTestCase : XunitDelayEnumeratedTheoryTestCase` (rows enumerated at run time); both are `ISelfExecutingXunitTestCase` and run through `CrucibleTestCaseRunner : XunitTestCaseRunnerBase<…>` whose context sends each `IXunitTest` to `CrucibleTestRunner : XunitTestRunnerBase<XunitTestRunnerContext, IXunitTest>` (`IsTestClassCreatable` false, `InvokeTest` runs the gate). Skip, explicit, before/after attributes, xUnit `Timeout` and the result messages stay xUnit's.
- **Per-run lifecycle** (`TestClassInstances`): factory (outside the run): single public constructor with xUnit's constructor arguments (fixtures, output helper, `Func<T>` unwrapped) via `TypeActivator`, then `IAsyncLifetime.InitializeAsync` (disposed and rethrown if it fails); body (inside the run): the method with the row's arguments (`Task`, `ValueTask`, `ValueTask<T>`, sync); teardown (outside the run, after the body): `IAsyncDisposable.DisposeAsync` (covers `IAsyncLifetime`) or `IDisposable.Dispose`. Static methods have no instance.
- **Engine (internal)**: `GateRequest(Groups, Declared, Func<int, Task<RunBody>> BodyFor, Sequential, CancellationToken Cancellation = default)`; `RunBody(Invoke, TeardownAsync)` with `RunBody.Of(body)`. Teardown runs after the body, outside the run, before the run settles; a teardown failure fails a passing run; a failing or null factory fails/usage-halts the run; bodies still running after a halt are torn down when they finish (possibly after the result). Cancellation: already-cancelled throws `OperationCanceledException` before any run; cancelled later ends the batch, closes runs, cancels the runs' tokens, rethrows `OperationCanceledException`. `Crucible.Engine` (internal) shares one engine with `Crucible.Gate`; `Crucible.UseJudge(IJudgeClient)` (internal) is the 9.7 `UseJudge(IChatClient)` groundwork and the fixture's no-network judge. No public engine API added; `Crucible.Gate` unchanged.
- **Config**: `build/Geekmotors.Crucible.Xunit.targets` packed to `build/` and `buildTransitive/` (verified with `dotnet pack`): `None Update` for `crucible.config.json`/`.jsonc` with `PreserveNewest`, plus an explicit include when default items are disabled. Imported directly by the fixture and live projects (the live project's manual copy rule removed). The missing-config error now names the folder searched and says the adapter copies the file from beside the `.csproj`.
- **Isolation tests** (`XunitAdapterTests`, one child run per TFM via a class fixture: `dotnet exec <fixture>.dll -reporter json -noColor -maxThreads unlimited`, working directory = fixture output, results parsed from the JSON messages): same class, parallel classes (cross-class rendezvous), parallel collections (named collections rendezvous), inline/ClassData/run-time-enumerated theory rows (one result each), N runs with their own instance and facts, facts never surviving to the next test, usage outside a run (plain `[Fact]`, constructor, static constructor, class fixture, collection fixture, `InitializeAsync`, `Dispose`), concurrent by default, `Sequential`, class/assembly defaults with method override, failing dispose, verdict exception and report as the failure message, skip, xUnit timeout and cancellation reaching the runs, static methods, invalid settings as `usage`. Facts are observed through what the fake judge receives (it echoes its prompt).
- **Live smoke**: `LiveAdapterTests` with one `[CrucibleFact]` and one `[CrucibleTheory]` row (runs 1, threshold 1, explicit, skipped unless `CRUCIBLE_JUDGE_KEY` via `SkipUnless`), one judge call each; the live project totals 4 calls. `make dotnet-live` unchanged (runs explicit tests). Not run.
- **Docs**: `dotnet/README.md` draft section (attributes, resolution, one result per row, lifecycle, isolation, timeouts/cancellation, config copy) and layout table; surface-check reasons for `crucible.it`/`describe` now point at the package.
- **Gates**: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` green (0 warnings; 3126 rows, 3046 passed, 80 skipped = the 74 spec ids plus the 6 explicit live rows on both TFMs; 3 consecutive runs); TS `npm test` 1477 passed; `make spec-goldens-check` green.
- **Open points**: a static initialiser of a helper type first touched inside a run runs inside that run (the test class's own static constructor is forced before each run, static test methods included).
- **Code review (2026-11-15)**: public `Crucible.Gate(body, cancellationToken)` / `Gate(options, body, cancellationToken)` added at the reviewer's direction (deviates from AC 1 "no public engine API"; closes deferred-work 9.5); see Review Findings.

### File List

- `_bmad-output/implementation-artifacts/9-6-dotnet-xunit-adapter.md` (new)
- `_bmad-output/implementation-artifacts/sprint-status.yaml`, `_bmad-output/implementation-artifacts/deferred-work.md`, `.gitignore` (modified)
- `dotnet/Directory.Packages.props`, `dotnet/Geekmotors.Crucible.slnx`, `dotnet/README.md` (modified)
- `dotnet/src/Geekmotors.Crucible/`: `Geekmotors.Crucible.csproj`, `Crucible.cs`, `Config/ConfigFileLocator.cs`, `Config/ConfigLoader.cs`, `Engine/EngineSession.cs`, `Engine/GateEngine.cs`, `Engine/GateRequest.cs`, `Engine/RunBatch.cs` (modified); `Engine/RunBody.cs` (new)
- `dotnet/src/Geekmotors.Crucible.Xunit/` (new): `Geekmotors.Crucible.Xunit.csproj`, `packages.lock.json`, `build/Geekmotors.Crucible.Xunit.targets`, `CrucibleAttribute.cs`, `CrucibleFactAttribute.cs`, `CrucibleTheoryAttribute.cs`, `Xunit/{CrucibleDelayEnumeratedTestCase,CrucibleFactDiscoverer,CrucibleSettings,CrucibleSettingsResolver,CrucibleTestCase,CrucibleTestCaseRunner,CrucibleTestCaseRunnerContext,CrucibleTestRunner,CrucibleTheoryDiscoverer,GatePlan,ICrucibleSettings,TestClassInstances}.cs`
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `Engine/RunLifecycleTests.cs` (new); `Constants/EngineRuns.cs`, `Engine/GateEngineTests.cs`, `Engine/RunIsolationTests.cs`, `Harnesses/CSharpSurface.cs`, `Harnesses/EngineHarness.cs`, `packages.lock.json` (modified)
- `dotnet/tests/Geekmotors.Crucible.Xunit.Tests/` (new): `Geekmotors.Crucible.Xunit.Tests.csproj`, `packages.lock.json`, `Constants/{FixtureTests,SampleValues}.cs`, `Harnesses/{FixtureOutcome,FixtureResult,FixtureRunHarness,PlanHarness,SingleThreadFixtureRunHarness}.cs`, `Isolation/{SingleThreadAdapterTests,XunitAdapterTests}.cs`, `Settings/CrucibleSettingsResolverTests.cs`, `Samples/{AssemblyDefaults,BaseSample,ClassDefaultsSample,DerivedSample,InvalidClassSample,OuterSample,OuterSample.InnerSample,OuterSample.SilentInnerSample,UndecoratedSample}.cs`
- `dotnet/tests/Geekmotors.Crucible.Xunit.Fixture/` (new): `Geekmotors.Crucible.Xunit.Fixture.csproj`, `packages.lock.json`, `crucible.config.json`, `Constants/{FixtureCollections,FixtureRuns,FixtureTexts}.cs`, `Fakes/PromptJudgeClient.cs`, `Harnesses/{AssemblySetup,ClassFixtureProbe,CollectionFixtureProbe,ErrorKindProbe,FactProbe,Meetings,ProbedCollectionDefinition,PromptJudgeInstaller,Rendezvous,Signals,StringRows,UnserializableRow}.cs`, `Isolation/{CollectionAlphaTests,CollectionBetaTests,ParallelLeftTests,ParallelRightTests,RunInstanceTests,SameClassTests,TheoryRowsTests}.cs`, `Lifecycle/{AsyncVoidTests,ConcurrentTests,FailingConstructorTests,FailingDisposalTests,FailingInitialiseTests,LifetimeTests,ReturnShapeTests,SequentialTests,StaticMethodTests,UnknownAwaitableTests}.cs`, `Results/{CancellationObserverTests,DynamicSkipTests,FailingVerdictTests,InvalidSettingsTests,SkippedTests,TimedOutTests,UsageErrorTests}.cs`, `Settings/{AssemblyDefaultsTests,ClassDefaultsTests}.cs`, `Usage/{ClassFixtureTests,CollectionFixtureTests,ConstructorTests,InjectionTests,PlainFactTests,StaticInitialiserTests}.cs`
- `dotnet/tests/Geekmotors.Crucible.LiveTests/`: `LiveAdapterTests.cs`, `LiveKey.cs` (new); `Geekmotors.Crucible.LiveTests.csproj`, `LiveSmokeTests.cs`, `LiveTexts.cs`, `packages.lock.json` (modified)
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `CrucibleTests.cs`, `ConsumerNamespaceTests.cs` (modified in review: pass the test token to `Crucible.Gate`)

### Review Findings

Code review 2026-11-15 (Blind Hunter, Edge Case Hunter, Acceptance Auditor). Patches applied:

- [x] [Review][Patch] `build/` gitignore rule swallowed `build/Geekmotors.Crucible.Xunit.targets` (fresh clone/CI build broken) [.gitignore]
- [x] [Review][Patch] Public `Crucible.Gate(..., CancellationToken)` overloads (both forms) with XML docs; engine tests for cancel-in-flight and pre-cancelled; existing callers pass `TestContext.Current.CancellationToken` (xUnit1051) [Crucible.cs, GateEngine.cs]
- [x] [Review][Patch] `Assert.Skip` / `SkipExceptions` inside a run failed the gate instead of skipping (AC 2) [CrucibleTestRunner.cs, TestClassInstances.cs]
- [x] [Review][Patch] Unknown awaitables (e.g. `YieldAwaitable`) silently passed; now refused like `async void` [CrucibleTestRunner.cs]
- [x] [Review][Patch] Static test methods could run the class's static initialiser inside a run; class constructor now forced in the factory [TestClassInstances.cs]
- [x] [Review][Patch] Constructor failure surfaced as `TargetInvocationException`; multiple public ctors gave a bare `InvalidOperationException`; an `InitializeAsync` failure was masked by a dispose failure [TestClassInstances.cs]
- [x] [Review][Patch] A run whose instance was still being created when the gate ended still ran its body (judge calls after the result); also no runs start when the token fires between the pre-check and registration [GateEngine.cs]
- [x] [Review][Patch] Config-copy targets overrode a user's own `CopyToOutputDirectory`; now only fills it when unset (verified: default items, `EnableDefaultItems=false`, `EnableDefaultNoneItems=false`, user `None Include`/`None Update`/`Content`, `.jsonc`; pack has `build/` + `buildTransitive/`) [Geekmotors.Crucible.Xunit.targets]
- [x] [Review][Patch] Fixture harness: child killed on hang guard, results without a start message ignored, all exception types kept [FixtureRunHarness.cs]
- [x] [Review][Patch] Missing proofs added: `async void`, return shapes (sync/`ValueTask`/`ValueTask<T>`/`Task<T>`), failing constructor/`InitializeAsync`, usage error from a run, `ITestOutputHelper` + assembly fixture injection, xUnit `Timeout` reports `TestTimeoutException`, `-maxThreads 1` run (no deadlock, runs still concurrent)
- [x] [Review][Patch] README: shared fixtures/row arguments/output helper between runs, runs outside xUnit's thread limit, skip semantics, timeout reporting, cancellable `Gate`
- [x] [Review][Defer] Orphan runs after halt/deadline/cancel can outlive xUnit's fixture disposal — deferred, decision needed (AC 6 documents it)
- [x] [Review][Defer] `Geekmotors.Crucible.Xunit` uses core internals but packs a `>=` dependency on `Geekmotors.Crucible` — deferred to packaging story (pin exact version)
- [x] [Review][Defer] Invalid attribute values name "a group"/"a test", not the attribute layer (AC 2 wording) — deferred
- [x] [Review][Defer] Teardown failure dropped when the body also failed; `UseJudge` after first engine use silently ignored; stale `.json` in bin after switching to `.jsonc`; fixture folder path assumes `bin/$(Configuration)/$(TargetFramework)` — deferred

### Change Log

- 2026-11-15: xUnit v3 adapter `Geekmotors.Crucible.Xunit` (`[CrucibleFact]`, `[CrucibleTheory]`, `[Crucible]`), per-run test-class instances with async setup and teardown, xUnit cancellation tied to the gate, config-copy build targets, real-xUnit isolation tests over a child-process fixture run, live smoke additions, README draft.

### Review decisions (2026-11-15, delegated)

- Runs outliving xUnit fixture disposal after a halt/deadline/cancel: documented now; a bounded grace period is a later story (deferred-work).
- Geekmotors.Crucible.Xunit uses Geekmotors.Crucible internals, so Story 9.8 packs it with an exact-version dependency on Geekmotors.Crucible ([x.y.z]).
- Accepted: public Crucible.Gate overloads taking a CancellationToken (deviation from AC 1; conventional .NET API for NUnit/MSTest gate users).
