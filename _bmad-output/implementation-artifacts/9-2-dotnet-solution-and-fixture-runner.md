---
baseline_commit: f931a6a
---

# Story 9.2: .NET solution and the fixture runner

Status: done

## Story

As the maintainer starting the C# port,
I want a `dotnet/` solution with the `Geekmotors.Crucible` library, an xUnit v3 test project and a fixture runner over the shared `spec/`,
so that every later .NET story proves its behaviour against the same answer key as TypeScript, and CI keeps it green on every PR.

## Acceptance Criteria

1. **Given** `dotnet/` with a solution, `src/Geekmotors.Crucible` (multi-target `net8.0`/`net10.0`) and `tests/Geekmotors.Crucible.Tests` (xUnit v3, both TFMs) **Then** a fixture runner loads `spec/**/*.json`, validates every fixture file against `spec/fixture.schema.json`, checks id prefixes (`<area>-<operation>-`) and global id uniqueness, decodes `$special` values (failing on an unknown token), and runs every case of `run-options/resolve`, `run-options/validate` and `verdict/compute` green through real C# code, one xUnit theory row per case id.
2. **And** operations not yet implemented in C# are listed in an explicit pending set (9.3–9.5 shrink it to empty); a fixture file whose `area`/`operation` is neither implemented nor pending fails a test.
3. **And** C# implements: the run options model (`Runs`, `Threshold`, `InferenceTimeout`, `TestTimeout`, `Strict`), resolution (built-in → testDefaults → groups outer→inner → test → call `strict`), validation per source (`test`/`group` → `usage`, `config` → `config`), the `testTimeout` formula in 64-bit, and the verdict (`need` with the exact evaluation order and `1e-9` epsilon). Public error type `CrucibleException` with `Kind` (`CrucibleErrorKind`: `Usage`, `Config`, `Infra`) and `Retryable` (`bool?`, `Infra` only).
4. **And** CI builds and tests .NET on every PR and push (Linux, both target frameworks) with a format/analyzer check (`dotnet format --verify-no-changes`, warnings as errors, `AnalysisLevel latest-recommended`).
5. **And** the TypeScript gate and `make spec-goldens-check` still pass untouched.

## Tasks / Subtasks

- [x] Scaffold (AC 1, 4) — exempt from TDD
  - [x] `dotnet/global.json` (SDK 10.0.x, `rollForward: latestFeature`, `test.runner: Microsoft.Testing.Platform`)
  - [x] `dotnet/Directory.Build.props`, `dotnet/Directory.Packages.props` (CPM), `dotnet/.editorconfig`
  - [x] `dotnet/Geekmotors.Crucible.slnx` (SDK 10 default format) with both projects
  - [x] `src/Geekmotors.Crucible/Geekmotors.Crucible.csproj`, `InternalsVisibleTo` the test project
  - [x] `tests/Geekmotors.Crucible.Tests/Geekmotors.Crucible.Tests.csproj` (xunit.v3, JsonSchema.Net test-only), `spec/**/*.json` embedded as resources
  - [x] root `.gitignore` (`bin/`, `obj/`, `TestResults/`), root `Makefile` `dotnet-test` (+ `dotnet-format`)
- [x] Fixture runner (AC 1, 2) in `tests/.../Spec/`
  - [x] embedded spec loader; schema validation; id prefix + uniqueness; operation coverage (implemented ∪ pending)
  - [x] `$special` decoder (NaN, ±Infinity, undefined, unreadable; unknown → fail)
  - [x] outcome capture (`value` / `error{kind,retryable}`, non-Crucible exception = test failure) and deep comparison (numbers as doubles, unset == missing)
  - [x] negative tests: malformed fixture rejected by the schema, wrong prefix, duplicate id, unknown special token
- [x] Engine core, TDD (AC 3)
  - [x] `CrucibleException`, `CrucibleErrorKind`
  - [x] `RunOptions` (public record, nullable fields), `ResolvedRunOptions`
  - [x] `RunOptionsValidator` (typed) and `RunOptionsReader` (JSON value → `RunOptions`), source → kind
  - [x] `RunOptionsResolver` (layers, formula in `long`, call strict)
  - [x] `VerdictCalculator` (`need`, zones, discarded)
- [x] Fixture theories for the three operations (AC 1)
- [x] CI job `dotnet` in `.github/workflows/ci.yml` (AC 4)
- [x] `dotnet/README.md` placeholder (AC 4)
- [x] Gates (AC 4, 5)

## Dev Notes

### Contract (spec/README.md wins over TS)

- **resolve**: precedence one field at a time; built-ins runs 10, threshold 0.9, inferenceTimeout 30000, strict false. `testTimeout` = nearest declared, else `min(2147483647, 30000 + (runs−1)×1000)` on the **resolved** runs, computed in `long`. Once any layer declares testTimeout the formula never applies. Call `strict` only; call present without strict → test's resolved strict.
- **validate**: input `{source, options: <any JSON>}`. Errors: not a plain object (null, array, number, string, bool; unset for test/group); unknown key (even unset); runs not integer ≥ 1 (null/strings/fractions/NaN/±∞); threshold not number in (0,1]; timeouts not integer 1..2147483647; strict not bool. A boolean is never a number. Config: unset options accepted as `{}`. Output equals input.
- **compute**: `need = max(1, ceil((threshold × runs) − 1e-9))` in `double`, in that order; runs = sum of all statuses (discarded included); outcome passed if passed ≥ need, else failed if passed + errored < need, else errored. Includes a 1,000,000-run case.
- **Runner**: decode `$special` recursively; compare by deep equality (keys any order, unset == missing, numbers as IEEE doubles, arrays in order). A thrown non-Crucible exception fails the case.

### C# design decisions

- `RunOptions` is a public `sealed record` with `int? Runs`, `double? Threshold`, `int? InferenceTimeout`, `int? TestTimeout`, `bool? Strict` (`null` = not declared, the spec's unset for C#). 9.5 `Crucible.Gate(options, …)` and 9.6 attributes feed it. `Runs` is `int`: the spec requires at least 2147483647; a JSON value above that is rejected with the source's kind.
- Two validation entry points, one rule set: `RunOptionsValidator.Validate(RunOptions?, RunOptionsSource)` (typed: ranges; `null` = unset options) and `RunOptionsReader.Read(JsonNode?, RunOptionsSource)` (JSON shape: object, known keys, JSON token kinds, integral values) delegating ranges to the validator. 9.3 uses the reader for `testDefaults` (`Config`).
- `$special: "undefined"` as a field value decodes to an absent field (spec: same as leaving it out). C# has no present-but-unset value, so `run-options-validate-{test,group}-unknown-key-unset` cannot be expressed: skip by id with a reason (README allows it). `options: undefined` at the top level goes through the typed validator with `null`.
- `VerdictCalculator.Compute(double threshold, IReadOnlyCollection<RunStatus>)` → `Verdict(VerdictOutcome, Need, Passed, Failed, Errored)`; the harness expands counts into a status list.
- Fixture files embedded as resources (PORTING §4: no file reads at test time) via an MSBuild glob over the repo `spec/`.

### Toolchain facts (verified by spike, 2026-11-15)

- SDK 10.0.404; `dotnet new sln` defaults to `.slnx`.
- `xunit.v3` 4.0.3 runs on Microsoft.Testing.Platform only: with SDK 10 `dotnet test` needs `global.json` `"test": { "runner": "Microsoft.Testing.Platform" }`; do not add `Microsoft.NET.Test.Sdk` / `xunit.runner.visualstudio` (VSTest mode errors). Test project needs `OutputType Exe`.
- `JsonSchema.Net` 9.7.0 validates draft-07 (`fixture.schema.json`) on net8.0 and net10.0; `Evaluate` takes a `JsonElement`.

### Testing standards (C#, CLAUDE.md in spirit)

TDD red-green-refactor; one type per file named after it; no comments except XML docs on public API; `sealed`/`record`; internals `internal` + `InternalsVisibleTo`; fakes in `Fakes/`, harnesses in `Harnesses/`, constants in their own files; assert outcome + error kind, never message text; tests mirror `src` folders.

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro, Story 9.2
- `spec/README.md` (Fixture format, Special values, Writing a runner, Operations)
- `spec/PORTING.md` §1 Run options, §4 Shared assets, §5 Language notes, §6 First week
- TS reference: `typescript/src/core/run-options-resolver.util.ts`, `run-options-validator.util.ts`, `aggregator.util.ts`, `time-budget.util.ts`, `declaration-forms.util.ts`, `config.ts` (`requireTestDefaults`)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- Spike: xunit.v3 4.0.3 + `Microsoft.NET.Test.Sdk` under SDK 10 fails ("Testing with VSTest target is no longer supported") → MTP runner via `global.json`.
- JsonSchema.Net registers schemas by `$id` in a global registry: building the schema twice throws → built once (lazy) in `SpecFixtureCatalog`.
- `$special: "undefined"` also appears as an array element (`assertion/call` args) → kept as the token for that operation's future harness; only object members are dropped.
- CA1822 (mark members static) disabled: stateless collaborators stay instance classes so 9.5 can inject them.

### Completion Notes List

- Runner: 238 fixture rows per TFM (resolve 38/38, validate 170/172 + 2 skipped by id, compute 28/28) in one theory `SpecCaseTests.Case(id)`; 32 other tests per TFM. After review: 540 total, 536 pass, 4 skipped (2 ids × 2 TFMs).
- Skipped: `run-options-validate-{test,group}-unknown-key-unset` — C# has no present-but-unset value.
- Fixtures embedded as resources (PORTING §4) instead of read from disk at test time.
- Own unit tests for the typed path were written together with the implementation rather than strictly red-first; the fixture theories were red (compile) before the implementation, and mutation checks (dropping the `1e-9` epsilon) turn fixture rows red.
- Gates: `dotnet build -warnaserror`, `dotnet test` (net8.0 + net10.0), `dotnet format --verify-no-changes` green; TS `lint/format:check/typecheck/test` (1477 tests) and `make spec-goldens-check` green.
- `actions/setup-dotnet` pinned by SHA (v6.1.0); dependabot `nuget` entry for `/dotnet`.

### File List

- `.github/workflows/ci.yml` (modified: `dotnet` job)
- `.github/dependabot.yml` (modified: `nuget` for `/dotnet`)
- `spec/PORTING.md` (modified: §2 xUnit collections note)
- `.gitignore` (modified: `dotnet/**/{bin,obj,TestResults}/`)
- `Makefile` (modified: `dotnet-build`, `dotnet-test`, `dotnet-format`)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (modified)
- `_bmad-output/implementation-artifacts/9-2-dotnet-solution-and-fixture-runner.md` (new)
- `dotnet/src/Geekmotors.Crucible/packages.lock.json`, `dotnet/tests/Geekmotors.Crucible.Tests/packages.lock.json` (new)
- `dotnet/.editorconfig`, `dotnet/Directory.Build.props`, `dotnet/Directory.Packages.props`, `dotnet/Geekmotors.Crucible.slnx`, `dotnet/global.json`, `dotnet/README.md` (new)
- `dotnet/src/Geekmotors.Crucible/`: `Geekmotors.Crucible.csproj`, `CrucibleErrorKind.cs`, `CrucibleException.cs`, `RunOptions.cs`, `Options/{ResolvedRunOptions,RunOptionsReader,RunOptionsResolver,RunOptionsSource,RunOptionsValidator}.cs`, `Verdicts/{RunStatus,Verdict,VerdictCalculator,VerdictOutcome}.cs` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/`: `Geekmotors.Crucible.Tests.csproj`, `CrucibleExceptionTests.cs`, `Constants/{MalformedFixture,OptionValues}.cs`, `Harnesses/{ErrorKindName,FixtureCase,FixtureChecker,FixtureFile,FixtureOutcome,FixtureProblem,JsonEquivalence,RunOptionsFixtureHarness,RunOptionsJson,SpecCaseRunner,SpecFixtureCatalog,SpecialValueDecoder,SpecOperations,UnknownSpecialTokenException,VerdictFixtureHarness}.cs`, `Options/{RunOptionsReaderTests,RunOptionsValidatorTests}.cs`, `Spec/{FixtureCheckerTests,SpecCaseTests,SpecFixtureTests,SpecialValueDecoderTests}.cs` (new)

### Review Findings

Code review 2026-11-15 (Blind Hunter, Edge Case Hunter, Acceptance Auditor). Applied:

- [x] [Review][Patch] Reader reads numbers of any CLR numeric type (`decimal`, `float`, …), uses `TryGetDouble` for element-backed values; out-of-double-range literal (`1e400`) rejected with the source kind (tests added red-first for the decimal case)
- [x] [Review][Patch] Reader key constants renamed `*Key` (no shadowing of `RunOptions` members)
- [x] [Review][Patch] `RunOptionsValidator.Place` maps `Config` explicitly; undefined source throws
- [x] [Review][Patch] Fixture discovery: every embedded `spec/**/*.json` is a fixture unless listed as a shared asset (`fixture.schema.json`, `surface.json`, `surface.schema.json`); guard test proves embedded files = fixtures ∪ shared assets; guard that every pending operation has fixtures
- [x] [Review][Patch] Implemented operations map to their harness; one theory over every implemented case, so an operation cannot be marked implemented without its cases running
- [x] [Review][Patch] Malformed fixture (missing id, wrong shape) parsed tolerantly and reported by the checker instead of crashing the catalog
- [x] [Review][Patch] Skip reason sharpened; reader test proves an unknown key is rejected whatever its value (`null`)
- [x] [Review][Patch] Decoder tests in their own class, no magic `"runs"`; harness shares one validator; `CrucibleException` retryable true/false both asserted
- [x] [Review][Patch] NuGet lock files (`RestorePackagesWithLockFile`), CI `restore --locked-mode` + setup-dotnet cache, `timeout-minutes`, `ContinuousIntegrationBuild` on Actions; `.gitignore` anchored to `dotnet/`
- [x] [Review][Patch] PORTING §2: xUnit note no longer forces one non-parallel collection (aligns with Epic 9 isolation and Story 9.6)
- [x] [Review][Defer] Duplicate JSON keys make `JsonObject` throw `ArgumentException`: 9.3 JSONC parser owns last-wins
- [x] [Review][Defer] Package metadata/SourceLink (9.8), surface check (9.5), README spec commit + skip list (9.7, PORTING §4)

### Change Log

- 2026-11-15: .NET solution, engine run options + verdict, spec fixture runner, CI job, Makefile targets.
- 2026-11-15: Code review fixes (see Review Findings).

### Review decisions (2026-11-15, delegated)

- Test/group validate fixtures keep running through the JSON reader (single JSON-shape rule set shared with config testDefaults in 9.3).
- From 9.3 on, unit tests are written red-first (TDD), not alongside the code.
- CA1822 stays off solution-wide (injectable instance collaborators).
