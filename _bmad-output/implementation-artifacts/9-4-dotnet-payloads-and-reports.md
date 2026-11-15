---
baseline_commit: dd9f0fd
---

# Story 9.4: Judge payloads and reports

Status: done

## Story

As the maintainer porting Crucible to C#,
I want the C# engine to build the judge's user message (fencing included), validate assertion calls, keep a per-run record of what was judged, and render the failure report and the debug trace from those records,
so that the .NET port sends exactly the payloads TypeScript and the conformance harness send, and prints exactly the same reports — proven by the shared fixtures.

## Acceptance Criteria

1. **Given** the 9.2 fixture runner **Then** every case of `payload/coherent` and `payload/extract` (incl. the generated `*-conformance.json` goldens), `report/render`, `report/trace` and `assertion/call` passes through real C# code on `net8.0` and `net10.0`, byte for byte; the five operations move from `SpecOperations.Pending` to `SpecOperations.Implemented`. A case is skipped only where C# types make its input inexpressible, by id, with the reason.
2. **And** payload text is built by an internal assembler and fence rule working on **code points** (README `payload` / `coherent` fencing: break set, invisible set incl. astral ranges, padding set, one U+0020 added), and reaches the judge through the 9.3 `JudgeExchange` with the schema file (`schemas/coherent.md` / `extract.md`) as the system message, word for word.
3. **And** `assertion/call` runs through internal assertion services (facts, coherent, extract: argument and option validation, call facts after run facts, `as`, `strict` over the test's strict) that 9.5's public `Crucible.Facts/CoherentAsync/ExtractAsync<T>` only wrap with the ambient run context. Fixture calls (JS-style args arrays) are mapped to C# typed calls in the harness.
4. **And** an internal run-record model (facts, judge calls in recorded order: verdicts, extractions with type and mode, errored calls with attempts; run results with status and cause; how the test ended) is filled by the assertion services (9.5's engine fills the rest) and read by the report renderer; the debug trace renders typed events, written to an injected output that can never fail the caller.
5. **And** text rules hold: UTF-16 code units for lengths and cuts (surrogate rule), `…` U+2026, `\n` only, numbers as ECMAScript `Number.prototype.toString` (own tested formatter, not `ToString("R")`), `Math.round` ties toward +∞, `JSON.stringify` escaping by an own writer.
6. **And** every C# language note in `spec/PORTING.md` §5 is checked against .NET 8 and .NET 10 with probes; wrong notes are corrected, unchecked ones stay marked *verify*.
7. **And** nothing new is public; unit tests are written red-first; gates: in `dotnet/` `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes`; TypeScript `npm test` and `make spec-goldens-check` untouched and green.

## Tasks / Subtasks

- [x] PORTING §5 probes (AC 6): scratch console app on both TFMs; correct the table
- [x] Text rules, TDD (AC 5): `EcmaScriptNumber`, `EcmaScriptMath.Round`, `EcmaScriptJson`, `TextFormat` (cut, indented lines)
- [x] Payloads, TDD (AC 2): `FenceNeutralizer`, `PayloadAssembler`, `PayloadSection`, `JudgeSchemas` (schemas embedded in the library)
- [x] Run records (AC 4): `RunRecord`, `JudgeCall` + `VerdictCall`/`ExtractionCall`/`ErroredCall`, `RunCause`, `RunResult`, `TestEnding` + kinds, `TestRecord`, `ExtractMode`
- [x] Assertion services, TDD (AC 3): `FactList`, `CoherentCall`, `ExtractCall`, `AssertionRun`, `ExtractTypes`, `CoherentAssertion`, `ExtractAssertion`
- [x] Report renderer (AC 1, 4, 5): `ReportSettings`, `ReportFields`, `ReportRenderer`
- [x] Debug trace (AC 1, 4, 5): `TraceSettings`, `ITraceOutput`, `StandardOutputTraceOutput`, `DebugTrace`, event records, `ProviderFailureNext`
- [x] Fixture harnesses + skip list; move five operations to Implemented (AC 1, 3)
- [x] Gates (AC 7)

### Review Findings

- [x] [Review][Decision] Which system prompt each assertion sends was untested — resolved: `EchoSystemJudgeClient` echoes `request.System` as the reasoning; `SendsTheCoherentSchemaAsTheSystemMessage` / `SendsTheExtractSchemaAsTheSystemMessage` compare the recorded reasoning with the repository `schemas/*.md` (outcome, no request capture; schema swap turns them red)
- [x] [Review][Decision] `Assert.IsType<VerdictCall>/<ExtractionCall>` kept — C# reading of the "never instanceof a concrete class" rule: the subtype is the `JudgeCall` union discriminant (which kind of call was recorded, observable behaviour), not an implementation identity
- [x] [Review][Patch] `DebugTrace` swallowed only IOException/ObjectDisposedException; now swallows every non-OOM sink exception, as TS `deliver` does [Tracing/DebugTrace.cs]
- [x] [Review][Patch] `EcmaScriptJson` printed non-finite numbers as `NaN`/`Infinity` and `GetDouble` threw on `1e999`; now `null` as `JSON.stringify`; `Value` accepts `double` only (no `IConvertible` cast failures) [Text/EcmaScriptJson.cs]
- [x] [Review][Patch] Skipped fixture cases never ran, so a stale skip passed silently; `SpecCaseRunner` now runs each skipped case and fails if it passes [tests/Harnesses/SpecCaseRunner.cs]
- [x] [Review][Patch] `assertion-call-extract-as-null` carried `NullIsUnsetReason`; now `TypeArgumentReason` [tests/Harnesses/SpecOperations.cs]
- [x] [Review][Patch] Fencing code-point boundary tests: every range endpoint of the break, invisible and padding sets plus outside neighbours [tests/Payloads/FenceNeutralizerTests.cs, tests/Constants/FenceCodePoints.cs]
- [x] [Review][Patch] More `Number::toString`/`Math.round` cases (>2^53, 2^64, 1e21 edges, subnormal boundary, ±.5 ties near 2^52) [tests/Text/EcmaScriptNumberTests.cs, EcmaScriptMathTests.cs]
- [x] [Review][Patch] Dead `SmallestPlainPoint` + literal `-6`; `++position > 0` side effect in `FirstFalseVerdict`; `??`/`?:` precedence in `ExtractAssertion` made explicit [Text/EcmaScriptNumber.cs, Reports/ReportRenderer.cs, Assertions/ExtractAssertion.cs]
- [x] [Review][Patch] Magic literals in `PayloadAssemblerTests` moved to `PayloadTexts` [tests/Payloads/PayloadAssemblerTests.cs]
- [x] [Review][Patch] PORTING §5 header claimed the whole C# column was probed; narrowed to the rows actually probed (all re-probed on net8 + net10) [spec/PORTING.md]
- [x] [Review][Defer] Assertion services never record `ErroredCall`; carry into 9.5 explicitly — deferred, 9.5 wires the exchange failure path
- [x] [Review][Patch] Root `.gitattributes` forces `eol=lf` for `schemas/*.md`, `spec/**/*.json`, `conformance/**`, `*.sh`; `git ls-files --eol` unchanged (all `i/lf w/lf`)
- [x] [Review][Defer] `Runs` <-> `Assertions` namespace cycle via `FactList` (same assembly, no runtime cycle) — deferred, low

## Dev Notes

### Contract (spec/README.md wins over TS; report disagreements)

- **Payload** (`payload/coherent`, `payload/extract`): sections `<HEADER>\n<<<\n<fenced>\n>>>` joined by `\n\n`; `STATE` only when run facts + call facts is non-empty (joined `\n`, not trimmed); then `RESPONSE`, `CLAIM` (coherent) or `RESPONSE`, `TARGET`, `TYPE`, `MODE` (extract). Fencing: split (check only) at U+000A–U+000D, U+001C–U+001E, U+0085, U+2028, U+2029; remove invisible code points (U+00AD, U+034F, U+061C, U+115F–U+1160, U+17B4–U+17B5, U+180B–U+180F, U+200B–U+200F, U+202A–U+202E, U+2060–U+206F, U+3164, U+FE00–U+FE0F, U+FEFF, U+FFA0, U+FFF0–U+FFF8, U+1BCA0–U+1BCA3, U+1D173–U+1D17A, U+E0000–U+E0FFF); trim padding (U+0009, U+0020, U+00A0, U+1680, U+2000–U+200A, U+202F, U+205F, U+3000); exactly `<<<`/`>>>` → prefix one U+0020 to the line as is. Code points, not units (`EnumerateRunes` turns a lone surrogate into U+FFFD: iterate by hand).
- **assertion/call** rules 1–6: facts call takes a string or an array of strings (else `usage`, assertion never called); first two args strings; options absent/unset = none, else an object with known keys only (`facts`; `as`/`strict`/`facts` for extract); `facts` option unset = none, call facts follow run facts and are not added to the run; `as` unset = `string`, else exactly `string`/`number`/`boolean`; `strict` unset keeps the test's, else bool and wins; `MODE` strict/lenient; coherent has no mode.
- **report/render**: headline table (smoke = `runs == 1`), `rate = floor(passed / runs × 1000 + 1e-9) / 1000`, verdict via `verdict/compute`, counts line, sections joined `\n\n`, `field`/`bounded` (excerptChars, facts joined `\n` with the extra `\n` before `…` when the next unit is `\n`), first failure rule, failed/errored/discarded sections, bound of three per status, ranges `#a-#b`, claim tally (most frequent first, ties in first-seen order, grouped by rendered label), full verbosity with judge calls. Report line breaks: `\r\n`, `\r`, `\n`, U+2028, U+2029; blank = ECMAScript trim set; trailing spaces/tabs removed per line in fields.
- **report/trace**: nothing below `debug`; header once before the first event line; `run #<n> ` tag; headline/body later lines indented 4 unless blank (trailing kept); `payload(label, value)` bound 2000 units; event table incl. `round(delayMs)` ties toward +∞; quoted values not escaped; meta keys in given order, values never written; `json(testDefaults)` compact in key order.

### C# design decisions

- Fixture calls → C# typed calls: `Facts(string)` / `Facts(IEnumerable<string>)` (fixture string / array), `CoherentOptions { Facts }`, `ExtractOptions { Facts, Strict }` + type from the generic argument (`as` → `typeof(string|double|bool)`; an unknown type name `date` → `typeof(DateTime)`, the C# form of an unsupported type). C# has one `null`, which README §Special values makes the unset value: fixture `null` where JS rejects `null` but accepts unset cannot be expressed. Internal call records keep nullable fields so a null string/fact is still a `usage` error (NRT is advisory).
- No untyped (JSON/`object[]`) entry point in `src/` just for fixtures (test-induced design; CLAUDE.md). Inexpressible cases are skipped by id with grouped reasons.
- System prompts: `schemas/*.md` embedded in the library assembly (no file reads at run time); a test proves they equal the repo files (embedded into the test assembly as well).
- Judge errors are not recorded as `ErroredCall` by the 9.4 services: the attempt count lives inside `ChatCompletionsClient`'s retry loop and needs the provider trace hook (provider-request/response/failure events), which 9.5 wires together with the ambient run number. The renderer and trace already read `ErroredCall`.
- `TraceSettings.TestDefaults` is the raw `JsonObject` (member order kept); 9.5 must hand it the config's raw `testDefaults` node.

### Testing standards

TDD red-first; one type per file; no comments except public XML docs; outcomes not interactions; fakes one per file in `Fakes/`, harnesses in `Harnesses/`, constants in `Constants/`; assert error kind only; no `\uXXXX` typed through heredocs (tooling turns them into raw characters — build escape-heavy strings from code points); no non-ASCII in `.cs` files; `ConfigureAwait(false)` in `src` (CA2007).

### Previous story intelligence (9.2, 9.3)

- `SpecOperation.Text` outcome; `SpecialValueDecoder` keeps `$special: undefined` inside arrays as the token (only object members are dropped) — the `assertion/call` harness maps an array token to C# `null`.
- `JudgeExchange.AskAsync(request, read, inferenceTimeout, cancellation)`; `CoherentReplyReader`, `ExtractReplyReader(ExtractType)`; `ManualTimeProvider` fake; `ExtractReply.Value` is `string`/`double`/`bool`/`null`.
- `EcmaScriptTrim` exists (trim set, `IsWhiteSpace(char)`).

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro, Story 9.4, 9.5
- `spec/README.md` §Text rules, §payload/coherent, §payload/extract, §report/render, §report/trace, §assertion/call, §Special values, §Out of scope
- `spec/PORTING.md` §1 Functions, Call options; §5
- TS: `typescript/src/core/{payload-assembler.util,reporter.util,trace-reporter,text-format.util,judge,extractor,fact-list.util,state}.ts`, harnesses `typescript/src/core/test/{payload,report,trace,assertion-call}-fixture-harness.util.ts`, `neutral-run.util.ts`

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- PORTING §5 probes: scratch console app (outside the repo) run on .NET 8.0.34 and 10.0.15 (macOS, ICU). Results: `EnumerateRunes` lone surrogate → U+FFFD (confirmed); `UnsafeRelaxedJsonEscaping` escapes astral as surrogate pairs, U+007F, U+2028/2029, uppercase hex; lone surrogate → `\uFFFD` (confirmed, extended); `ToString("R")` switches to E from 1E+17 (not 1E+15) and below 1E-04, two-digit exponent, `-0` (corrected); `Math.Round` banker's, `Floor(x + 0.5)` → 1 for 0.49999999999999994 (confirmed), `ToPositiveInfinity` is a ceiling (added); `ToLowerInvariant` leaves U+0130 unchanged with ICU (corrected; invariant globalization mode left *verify*: `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` did not take effect under `dotnet run`); `Trim` U+0085 in / U+FEFF out (confirmed); `^...$` matches before a trailing `\n` (added).
- Tooling: `\uXXXX` typed through the Write tool and heredocs is turned into raw characters inconsistently; every non-ASCII test string is built from code points (`Constants/UnicodeTexts.cs`), src uses `(char)0x2026` style constants. No non-ASCII in any new `.cs` file.
- xUnit1025: `0d` and `-0d` InlineData count as duplicates; negative zero has its own test.
- First lone-surrogate JSON test used U+D800 U+DC00, which is a valid pair: fixed to low-then-high.

### Completion Notes List

- Five operations moved from `Pending` to `Implemented`; `Pending` is now empty (no operation needs the engine to run its fixtures). Per TFM (net8.0 and net10.0 alike): `payload/coherent` 52/52 (43 + 9 conformance goldens), `payload/extract` 48/48 (16 + 32 goldens), `report/render` 58/58, `report/trace` 24/24, `assertion/call` 34/69 run green + 35 skipped by id. Totals both TFMs: 2202 rows, 2128 pass, 74 skipped (37 ids x 2).
- `assertion/call` skips (grouped reasons in `SpecOperations`): not a string (4: coherent-actual-number, coherent-expectation-object, extract-text-number, extract-description-number); missing required argument (2: coherent-expectation-missing, extract-description-missing); null is the C# unset value (6: coherent-options-null, coherent-facts-null, extract-options-null, extract-as-null, extract-strict-null, extract-facts-null); typed options record (9: coherent-options-string/-array, coherent-unknown-option, coherent-extract-option, coherent-unknown-option-unset, extract-options-string/-array, extract-unknown-option, extract-unknown-option-unset); not a string list (11: coherent-facts-number/-object/-mixed-array/-nested-array, facts-number/-object/-mixed-array, facts-error-after-valid-call, extract-facts-number/-object, extract-run-facts-invalid); type is the generic argument (1: extract-as-wrong-casing); strict is `bool?` (2: extract-strict-string/-number). Expressible and green: null/unset actual, text, facts (`usage`), `["x", null]` call facts, `as: "date"` → `typeof(DateTime)` (`usage`).
- No untyped entry point was added to `src/` for the skipped cases (would be test-induced design): the harness maps fixture args to the typed internal call records and throws `InexpressibleInputException` for anything C# cannot express, so a skipped id that becomes expressible fails loudly.
- Payload: `FenceNeutralizer` works on code points (hand-iterated pairs; a lone surrogate is neither invisible nor padding), `PayloadAssembler`, schemas embedded in the library (`schemas/*.md`, logical names `schemas/<file>`), proven equal to the repo files. Payload fixtures run through the real `CoherentAssertion`/`ExtractAssertion` and `JudgeExchange` with an echo judge (`EchoJudgeClient`) and read the payload back from the recorded call, as TS does.
- Assertion services (internal, for 9.5 to wrap): `RunRecord.AddFacts` (validates via `FactList`, copy, append-only, thread-safe), `CoherentAssertion.AskAsync(AssertionRun, CoherentCall)` → `bool`, `ExtractAssertion.AskAsync(AssertionRun, ExtractCall)` → `object?` (`string`/`double`/`bool`/`null`); `ExtractCall.As` is a CLR `Type` (`null` → string; `string`, `double`/`double?`, `bool`/`bool?`; else `usage`) so 9.5's `ExtractAsync<T>` passes `typeof(T)`. Facts are read when the call starts (unit-tested with a held judge reply); call facts are not added to the run.
- Run records: `RunRecord` (facts + judge calls in order), `JudgeCall` → `VerdictCall`/`ExtractionCall`/`ErroredCall`, `RunCause(Name, Message)`, `RunResult`, `TestEnding` → `VerdictEnding`/`AbortedEnding(FatalRun)`/`ExpiredEnding(DeadlineMs)`, `TestRecord`. The services record verdicts and extractions; `ErroredCall` (needs the provider attempt count) is recorded by 9.5 when it wires the provider trace hook.
- Report: `ReportRenderer(ReportSettings).Render(TestRecord)` and `ReportFields`; debug trace: `DebugTrace(TraceSettings, ITraceOutput).Write(TraceEvent)` with ten event records (+ `ProviderFailureNext`), header once under a lock, output failures (`IOException`, `ObjectDisposedException`) swallowed; `StandardOutputTraceOutput` (untested one-liner). The passing-report write (`full`/`debug` + `\n`) and emitting events from the provider client/exchange are 9.5 (engine) work.
- Text: `EcmaScriptNumber` (digits from `"R"`, ECMAScript layout), `EcmaScriptMath.Round`, `EcmaScriptJson` (own escaper; values and compact `JsonNode`), `TextFormat` (cut, report line breaks, indented continuation, blank via `EcmaScriptTrim`).
- `TraceSettings.TestDefaults` is a raw `JsonObject` (member order as written); `JudgeConfig` still only keeps the parsed `RunOptions`, so 9.5 must keep the raw `testDefaults` node from `ConfigLoader` (one-line change) to print it in the given order.
- README vs TS: no disagreement found.
- Gates: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` green (0 warnings); TS `npm test` 1477 passed; `make spec-goldens-check` green. No new packages, lock files unchanged.

### File List

- `_bmad-output/implementation-artifacts/9-4-dotnet-payloads-and-reports.md` (new)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (modified)
- `spec/PORTING.md` (modified: §5 C# notes verified/corrected)
- `dotnet/src/Geekmotors.Crucible/Geekmotors.Crucible.csproj` (modified: schemas embedded)
- `dotnet/src/Geekmotors.Crucible/Assertions/`: `AssertionRun`, `CoherentAssertion`, `CoherentCall`, `ExtractAssertion`, `ExtractCall`, `ExtractTypes`, `FactList` (new)
- `dotnet/src/Geekmotors.Crucible/Judging/`: `ExtractMode`, `ExtractWords` (new)
- `dotnet/src/Geekmotors.Crucible/Payloads/`: `FenceNeutralizer`, `JudgeSchemas`, `PayloadAssembler`, `PayloadSection` (new)
- `dotnet/src/Geekmotors.Crucible/Reports/`: `ReportFields`, `ReportRenderer`, `ReportSettings` (new)
- `dotnet/src/Geekmotors.Crucible/Runs/`: `AbortedEnding`, `ErroredCall`, `ExpiredEnding`, `ExtractionCall`, `JudgeCall`, `RunCause`, `RunRecord`, `RunResult`, `TestEnding`, `TestRecord`, `VerdictCall`, `VerdictEnding` (new)
- `dotnet/src/Geekmotors.Crucible/Text/`: `EcmaScriptJson`, `EcmaScriptMath`, `EcmaScriptNumber`, `TextFormat` (new)
- `dotnet/src/Geekmotors.Crucible/Tracing/`: `DebugTrace`, `ITraceOutput`, `JudgeRecordedEvent`, `JudgeReplyRejectedEvent`, `ProviderFailureEvent`, `ProviderFailureNext`, `ProviderRequestEvent`, `ProviderResponseEvent`, `RunSettledEvent`, `StandardOutputTraceOutput`, `TestAbortedEvent`, `TestExpiredEvent`, `TestStartedEvent`, `TraceEvent`, `TraceSettings`, `VerdictReachedEvent` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Geekmotors.Crucible.Tests.csproj` (modified: repo schemas embedded for comparison)
- `dotnet/tests/Geekmotors.Crucible.Tests/Assertions/`: `CoherentAssertionTests`, `ExtractAssertionTests` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Constants/`: `AssertionTexts`, `TraceTexts`, `UnicodeTexts` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Fakes/`: `BrokenTraceOutput`, `EchoJudgeClient`, `RecordingTraceOutput` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Harnesses/`: `AssertionCallFixtureHarness`, `AssertionHarness`, `CallArguments`, `InexpressibleInputException`, `PayloadFixtureHarness`, `ReportFixtureHarness`, `RepositorySchemas`, `RunRecordJson`, `TraceFixtureHarness` (new); `SpecOperation`, `SpecOperations` (modified)
- `dotnet/tests/Geekmotors.Crucible.Tests/Payloads/`: `FenceNeutralizerTests`, `JudgeSchemasTests`, `PayloadAssemblerTests` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Runs/RunRecordTests.cs` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Text/`: `EcmaScriptJsonTests`, `EcmaScriptMathTests`, `EcmaScriptNumberTests`, `TextFormatTests` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Tracing/DebugTraceTests.cs` (new)

### Change Log

- 2026-11-15: Payload assembly with code-point fencing, assertion services (facts, coherent, extract), run records, report renderer and debug trace in C#; `payload/*`, `report/*`, `assertion/call` implemented (35 `assertion/call` ids skipped as inexpressible); PORTING §5 C# notes verified and corrected.
