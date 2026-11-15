---
baseline_commit: 550d44d
---

# Story 9.3: Parsing, config and providers

Status: done

## Story

As the maintainer porting Crucible to C#,
I want the C# engine to parse JSONC config files, load and check the config, build and send the judge's HTTP requests, classify responses and retries, decide the plain-http warning and read the judge's reply,
so that the .NET port talks to the same judges, with the same config file and the same failure rules, as TypeScript — proven by the shared fixtures.

## Acceptance Criteria

1. **Given** the 9.2 fixture runner **Then** every case of `jsonc/parse`, `config/load`, `provider/request` (incl. the generated `provider/request-conformance.json` goldens), `provider/response`, `provider/plain-http-warning` and `judge/reply` passes through real C# code on `net8.0` and `net10.0`; the six operations move from `SpecOperations.Pending` to `SpecOperations.Implemented`. A case is skipped only where C# genuinely cannot express its input, by id, with the reason.
2. **And** HTTP goes through an injected `HttpMessageHandler`/`HttpClient` (production: one shared `HttpClient`, no client timeout — deadlines belong to the caller); the request fixtures observe the request the way a user would: a fake HTTP boundary answers with the request itself as the judge reply, read back through the real provider client.
3. **And** config loading runs behind injected boundaries: `IConfigFileSystem` (read text / exists; missing vs unreadable as `lstat` would tell) and `IEnvironment` (read one variable), each with a real implementation and one fake per file. Retry backoff and the inference deadline use injected time (`TimeProvider` + a backoff boundary), so no test sleeps.
4. **And** secrets never appear in any error (message, `ToString()`, every `InnerException`): key-like field values, config file text, the judge key, credentials; `config` / `load` errors never echo the `apiKeyEnv` or `baseUrl` value. (Review decision: a validated `apiKeyEnv` name may be named in the missing/bad-key error; the `baseUrl` host/port may appear in a network-failure cause.)
5. **And** nothing new is public: all types are `internal` (9.5 composes them); `CrucibleException`/`CrucibleErrorKind`/`RunOptions` stay the only public surface.
6. **And** unit tests (written red-first) cover what fixtures cannot: the boundary classifications, the WHATWG URL rules beyond the fixture sample, the once-per-client plain-http warning that never holds the key, cancellation and the inference deadline (not re-asked), the backoff schedule.
7. **And** gates: in `dotnet/` `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes`; TypeScript `npm test` and `make spec-goldens-check` untouched and green.

## Tasks / Subtasks

- [x] Runner plumbing (AC 1, 2): async operations (`Func<JsonNode?, Task<JsonNode?>>`), text outcomes (`provider/response`), skip list unchanged unless justified
- [x] JSON text parser, TDD (AC 1): one left-to-right scanner, two dialects — `Jsonc` (config) and `Strict` (provider envelopes, judge replies); `JsonNode` output preserving member order, duplicate keys last-wins, numbers as doubles
- [x] ECMAScript `trim` (AC 1): exactly the README set
- [x] WHATWG URL parser for `http`/`https` (AC 1, 6): trimming, tab/newline removal, scheme, slashes (`\` as `/`), credentials detection, host (percent-decode, IDNA, forbidden code points, IPv4 any WHATWG form, IPv6 with compression), port (default dropped), path (dot segments, percent-encoding), `href`
- [x] Config boundaries (AC 3): `IConfigFileSystem`/`PhysicalConfigFileSystem`, `ConfigFileLocator` (missing vs unreadable), `IEnvironment`/`ProcessEnvironment`
- [x] Config loader (AC 1, 4): rules 1–15 in order; `JudgeConfig`
- [x] Provider request (AC 1, 2, 4): endpoint per provider, key variable + trim + printable ASCII, headers, body member order (meta, `model`, `messages`)
- [x] Provider response + retries (AC 1, 3): attempt classification, 3 attempts, injected backoff
- [x] Plain-http warning (AC 1, 6): rule on the parsed host; written once per client, never holding the key
- [x] Judge reply (AC 1, 6): unwrap, strict parse, shapes, re-ask once, deadline per ask (timeouts not re-asked)
- [x] Gates (AC 7)

### Review Findings

- [x] [Review][Decision] (resolved: keep inner exception, AC4 narrowed) Network failure host:port in the inner exception — the retryable `infra` error keeps the last `HttpRequestException` as `InnerException`; SocketsHttpHandler messages carry `(host:port)` of the baseUrl. AC4 lists "baseUrl text" literally; README rule 7 forbids echoing the value because it "may hold a password" (credentials are rejected, so host:port holds none). TS keeps the fetch cause. Options: (a) keep the inner exception for diagnostics and narrow AC4 to "never the configured value / credentials / path" (recommended, TS parity); (b) drop the inner exception for network failures.
- [x] [Review][Decision] (resolved: keep naming the variable, AC4 item scoped to config/load) Missing/bad key errors name the `apiKeyEnv` variable — `JudgeKeyReader` says "Set the <apiKeyEnv> environment variable…". AC4 lists "`apiKeyEnv` values"; README rule 8 ("never echoed") is a `config` / `load` rule for an invalid value. By then the name has passed `[A-Za-z_][A-Za-z0-9_]*`. TS does the same. Options: (a) keep naming the variable (recommended: the user must know which variable to set) and scope AC4 to `config` / `load`; (b) say "the variable apiKeyEnv names".
- [x] [Review][Decision] (resolved: documented deviation, here and PORTING.md §5) Lone surrogates in prompt/system/meta are sent as U+FFFD — `Utf8JsonWriter` replaces an unpaired surrogate with `\uFFFD`; TS `JSON.stringify` sends `\udXXX`. No crash (checked on net8.0 and net10.0); README fixtures never hold a lone surrogate. Options: (a) accept and document as a port deviation (recommended); (b) a hand-written writer that escapes lone surrogates like `JSON.stringify`.
- [x] [Review][Decision] (resolved: documented deviations, here and PORTING.md §5) Non-ASCII host IDNA strictness — `IdnMapping` (ICU, and also in invariant globalization mode) rejects empty labels (`a..bücher`), CheckHyphens violations and over-long labels that WHATWG `domainToASCII` (CheckHyphens/VerifyDnsLength off) accepts; a `baseUrl` host that WHATWG accepts but `System.Uri` cannot hold (`{`, `}`, `` ` ``, `!`, `~`, …) is now a `config` error at client construction. Options: (a) document both as port deviations in the story and PORTING.md §5 (recommended); (b) hand-write UTS46 (large).
- [x] [Review][Patch] Strict/JSONC parser recursed without a depth limit: a deeply nested provider body or judge reply overflowed the stack and killed the test host [Json/JsonTextParser.cs] — nesting deeper than 512 is now a syntax error (retryable for a provider body, unusable for a reply, `config` for the file)
- [x] [Review][Patch] `IOException` (e.g. `HttpIOException`) from an attempt escaped raw instead of being retried [Providers/ChatCompletionsClient.cs]
- [x] [Review][Patch] A failure observed after cancellation escaped as an internal `ProviderAttemptException`/`HttpRequestException`; it now ends as `OperationCanceledException` (so the exchange maps a deadline to the retryable timeout) [Providers/ChatCompletionsClient.cs]
- [x] [Review][Patch] A response declaring an unknown/invalid charset threw `InvalidOperationException`; the body is now decoded as UTF-8 whatever the charset (leading BOM dropped), as `fetch().text()` does [Providers/ChatResponseReader.cs]
- [x] [Review][Patch] A WHATWG-valid host that `System.Uri` rejects threw `UriFormatException` from the client constructor; now `config` [Providers/ChatEndpoint.cs]
- [x] [Review][Patch] Raw U+FEFF characters in C# sources [Json/JsonTextParser.cs, tests Constants/JsonTexts.cs]
- [x] [Review][Patch] Syntax-error line numbers ignored CR-only and CRLF breaks consistently [Json/JsonTextParser.cs]
- [x] [Review][Patch] Inference-timeout message formatted with the current culture [Judging/JudgeExchange.cs]
- [x] [Review][Patch] Plain-http warning named the `System.Uri` host instead of the WHATWG-parsed host that decided the warning [Providers/ChatCompletionsClient.cs]
- [x] [Review][Patch] Shared production `HttpClient` never refreshed DNS: `SocketsHttpHandler` with `PooledConnectionLifetime` 2 min, still no client timeout [Providers/ChatCompletionsClient.cs]
- [x] [Review][Patch] `ConfigureAwait` convention: library code awaits with `ConfigureAwait(false)`, enforced by CA2007 (warning, so error under `-warnaserror`) for `src/**.cs` only (xUnit forbids it in tests) [.editorconfig, src awaits]
- [x] [Review][Patch] `HttpAnswers` (static answer builders, not a fake) moved from `Fakes/` to `Harnesses/`
- [x] [Review][Patch] Test fakes keyed by `/`-joined paths while `ConfigFileLocator` uses `Path.Combine` (Windows-only break) [Harnesses/ConfigFixtureHarness.cs, Harnesses/ProviderHarness.cs, Constants/ConfigPaths.cs]
- [x] [Review][Patch] Backoff schedule test covered only the second retry; first retry (250 ms) added [Providers/ExponentialBackoffTests.cs]
- [x] [Review][Patch] Scope reduction: `Ipv4Parser` drops `BigInteger` for a bounded `ulong` (any part above 2^32 fails WHATWG anyway) [Urls/Ipv4Parser.cs]
- [x] [Review][Defer] 429 `Retry-After` is ignored (3 attempts within ~1.75 s against a rate-limited judge) [Providers/ChatCompletionsClient.cs] — deferred, contract fixes the schedule
- [x] [Review][Defer] A deleted working directory makes `Directory.GetCurrentDirectory()` throw a raw `IOException` from `ConfigFileLocator.Location` [Config/ConfigFileLocator.cs] — deferred, outside the contract
- [x] [Review][Defer] The once-per-client warning test is sequential; the `Interlocked` guard is not exercised concurrently [tests Providers/ChatCompletionsClientTests.cs] — deferred

## Dev Notes

### Contract (spec/README.md wins over TS; report disagreements)

- `jsonc/parse` (README §jsonc/parse): BOM only at index 0; strings end at the next unescaped `"` (a `\` always takes the next unit), valid JSON escapes, no raw control chars; `//` to CR/LF (U+2028 does not end it); `/* */` non-nesting, `/*/` does not close, separates tokens (`1/**/0` error); trailing comma dropped only after a value before `]`/`}`; whitespace only SP/TAB/LF/CR; duplicate keys last-wins; empty/whitespace/comments-only error; any root. Every failure `config`; the error never holds the text (a line number is fine).
- `config/load` rules 1–15 **in order** (first failure decides): files (unreadable → error; none → error; both → error) → syntax → root object → key-like names (`ToLowerInvariant` ∈ `apikey, api_key, key, token, secret, authorization`, value never echoed) → unknown fields (case-sensitive; `$schema` ignored whatever its value) → `provider` non-blank (ECMAScript trim) → `baseUrl` (raw has no `?`/`#`; WHATWG absolute http(s), no username/password; resolved = `href` minus trailing `/`s; never echoed) → `apiKeyEnv` (`\A[A-Za-z_][A-Za-z0-9_]*\z`, never echoed) → provider name exact + `baseUrl` required/forbidden → `model` non-blank, kept verbatim → `meta` object → `verbosity` → `CRUCIBLE_VERBOSE` (non-empty must be exact level → else **usage**) → `testDefaults` (`RunOptionsReader`, source `Config`) → `excerptChars` (whole number ≥ 1). Optional field `null` is an error.
- `provider/request`: URL `<base>/chat/completions` (openrouter `https://openrouter.ai/api/v1`, openai `https://api.openai.com/v1`, compatible resolved `baseUrl`); key from `apiKeyEnv` or `CRUCIBLE_JUDGE_KEY` only, ECMAScript-trimmed, blank = none; openrouter/openai need one (`config`), compatible only with `apiKeyEnv`; a set key must be U+0021–U+007E (else `config`, key never echoed) even where optional; headers exactly `Authorization: Bearer <key>` (when sent) and `Content-Type: application/json` (no charset); body = meta members, then `model`, then `messages` [system, user] (meta `model`/`messages` replaced).
- `provider/response`: 2xx → strict-JSON body; non-null `error` member → numeric `code` classified as a status (fatal iff 400 ≤ code < 500 and not 408/429, fractions too), else retryable; else `choices[0].message.content` string (empty ok, exact) else retryable. Other status: fatal 4xx except 408/429; everything else retryable; network failure retryable. Max 3 attempts; fatal ends at once (`infra`, `retryable:false`); third retryable → `infra`, `retryable:true`.
- `provider/plain-http-warning`: true iff key sent ∧ scheme `http` ∧ parsed host not loopback (`localhost` domain, IPv4 127/8 after WHATWG IPv4 parsing, IPv6 exactly `::1`; `localhost.`, `foo.localhost`, `[::ffff:127.0.0.1]` are remote).
- `judge/reply`: ECMAScript trim; strip leading ```` ``` ```` + ASCII letters + trim-set whitespace; strip trailing ```` ``` ```` + whitespace; trim; strict JSON (last wins, `1e999` → ∞ → unusable); coherent `{verdict: bool, reasoning: string}`, extract `{value: present, null | string | finite number | bool per type, reasoning: string}`; unusable → re-ask once; second unusable → `infra` retryable; provider errors end the call unchanged.

### C# design decisions

- In-house JSON parser (`Json/JsonTextParser`, dialect enum): no dependency, one scanner for JSONC and strict JSON; `System.Text.Json` cannot do JSONC rule 4/5 exactly nor last-wins duplicates on `net8.0`. Numbers become `double` `JsonValue`s. A JSONC number that overflows to ±∞ is a `config` error (contract: unspecified; an infinite number cannot be sent as JSON). Strict mode keeps ∞ (reply rule 2).
- `TimeProvider` (BCL, net8.0+) for the inference deadline; `IRetryBackoff` for the wait between attempts (`ExponentialBackoff`: 250 ms × 2^(retry−1) × (1 + jitter), as TS). Tests use `ManualTimeProvider` and `ImmediateBackoff` fakes.
- Port deviations (review decisions, also in `spec/PORTING.md` §5): `IdnMapping` rejects empty labels, CheckHyphens violations and over-long labels that WHATWG accepts; a WHATWG-valid host `System.Uri` cannot hold (`{`, `}`, `` ` ``, `!`, `~`, …) is a `config` error when the client is built; an unpaired surrogate in the prompt, system prompt or meta is sent as `\uFFFD` (TS: `\udXXX`).
- WHATWG host parsing is hand-written (`Uri` lower-cases differently, accepts credentials, treats IPv4-mapped as loopback, does not read `127.1`/`0x7f.1`). Non-ASCII hosts go through `IdnMapping` (ICU UTS46) — documented deviation: `xn--` labels in ASCII input are not validated.
- Real boundary implementations: `PhysicalConfigFileSystem` reads UTF-8 bytes without stripping the BOM (Node `readFileSync('utf8')` keeps it); `Exists` is `lstat`-like (a dangling symlink exists). `ProcessEnvironment` is a one-line adapter, untested (tests never touch real process state).

### Testing standards

TDD red-first (review decision 9.2); one type per file; no comments except public XML docs; outcomes not interactions; fakes one per file in `Fakes/`, harnesses in `Harnesses/`, constants in `Constants/`; assert error kind, never message text; never inspect captured requests in unit tests (fixtures observe the request through the echo boundary, as TS does).

### Previous story intelligence (9.2)

- MTP runner (`global.json`), xunit.v3 4.0.3, CPM + lock files (`--locked-mode`), fixtures embedded as resources, `SpecOperations.Implemented` drives one theory over every implemented case; unknown special token fails; non-Crucible exceptions fail a case.
- `RunOptionsReader.Read(JsonNode?, RunOptionsSource.Config)` handles `testDefaults` (deferred from 9.2: duplicate keys belong to this story's parser — the parser builds `JsonObject`s with last-wins assignment so `JsonObject` never sees a duplicate).
- CA1822 off; `InternalsVisibleTo` the test project.

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9 intro, Story 9.3
- `spec/README.md` §Writing a runner, §Text rules, §jsonc/parse, §config/load, §provider/request, §provider/response, §provider/plain-http-warning, §judge/reply; §Generated goldens
- `spec/PORTING.md` §1 Config file, Errors; §3; §5 language notes (trim, lower-casing, regex, URLs, JSON input, headers)
- TS reference: `typescript/src/core/{jsonc-reader.util,config,file-system-config-boundary,retry,judge-exchange}.ts`, `typescript/src/providers/{chat-completions-adapter,openai-compatible,transport-security.util}.ts`

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- `System.Uri` canonicalises paths (`/%41pi` → `/Api`, `%2e%2e` resolved): the endpoint `Uri` is built with `UriCreationOptions.DangerousDisablePathAndQueryCanonicalization` so the WHATWG-serialised path is sent verbatim (test `SendsToThePathExactlyAsTheBaseUrlSerialises`, red before the fix).
- `JsonNode?["x"]` throws on a non-object root: envelope reading pattern-matches `JsonObject` first (caught by `provider-response-malformed-envelope-root-*`).
- Tooling: `\uXXXX` escapes typed into heredocs were turned into raw characters (a raw U+2028 broke a C# literal); escape-heavy files were generated with `chr(92)`; no non-ASCII remains in any `.cs` file.

### Completion Notes List

- All six operations moved from `Pending` to `Implemented`; per TFM (net8.0 and net10.0 alike): `jsonc/parse` 63/63, `config/load` 140/140, `provider/request` 57/57 (52 + 5 conformance goldens), `provider/response` 52/52, `provider/plain-http-warning` 20/20, `judge/reply` 65/65. No new skips (the two 9.2 skips remain). Totals: 1482 rows, 1478 pass, 4 skipped (2 ids x 2 TFMs).
- Runner: operations are async (`SpecOperation` with a `value`/`text` outcome key); `provider/response` captures `text`. Request fixtures observe the request through an echo HTTP boundary read back through the real client (as TS); header names compared case-insensitively, client-added headers ignored.
- Internal types only; public surface unchanged. Boundaries: `IConfigFileSystem` (`PhysicalConfigFileSystem`: UTF-8 without BOM stripping, lstat-like `Exists`), `ConfigFileLocator` (missing vs unreadable), `IEnvironment` (`ProcessEnvironment`, untested one-liner: tests never touch real process state), `HttpClient`/`HttpMessageHandler` (production: one shared `HttpClient`, infinite client timeout), `IRetryBackoff` (`ExponentialBackoff`: 250 ms x 2^(retry-1) x (1 + jitter) on `TimeProvider`), `TimeProvider` for the per-ask inference deadline, `IWarningSink` (`StandardErrorWarningSink`).
- In-house JSON scanner (`JsonTextParser`, `JsonDialect.Jsonc`/`Strict`): last-wins duplicates, doubles; JSONC rejects a number overflowing to infinity (contract unspecified; infinity cannot be sent as JSON); strict keeps it (reply rule 2).
- WHATWG URL parser for http/https (`HttpUrlParser`, `HostParser`, `Ipv4Parser`, `Ipv6Parser`, `PercentEncoding`), unit-tested beyond the fixture sample (backslashes, tab/newline removal, trimming, percent-encoding, IDNA, IPv4 octal/hex/shorthand, IPv6 compression and embedded IPv4, ports, empty credentials). Deviations: non-ASCII hosts use `IdnMapping` (ICU UTS46), ASCII `xn--` labels are not validated; `^` is in the path percent-encode set (current standard; Node may differ); the parser returns no URL for input with a query or fragment (config rejects raw `?`/`#` first).
- Unit tests written red-first (skeleton throwing `NotImplementedException` or compile-red, then green): parser dialects, trim set, URL rules, boundary classification (fake and real file system; the real one only reads the test output folder, writes nothing), excerptChars above 32 bits, plain-http warning once per client and never holding the key, no warning over https, cancellation not retried, exact path on the wire, backoff schedule, inference deadline is retryable infra and not re-asked, run cancellation is not a timeout.
- README vs TS: no behavioural disagreement found on the fixtures. Notes: TS sends only a user message when no system prompt is given (README always both; Crucible always has one); TS accepts `excerptChars` up to 2^53, C# up to 2147483647 (README: "at least 2147483647"); TS JSONC accepts `1e999` (Infinity, sent as `null` in meta), C# rejects it as `config`.
- Gates: `dotnet restore --locked-mode && dotnet build --no-restore -warnaserror && dotnet test --no-build && dotnet format --verify-no-changes` green; TS `npm test` 1477 passed; `make spec-goldens-check` green. No new packages.

### File List

- `_bmad-output/implementation-artifacts/9-3-dotnet-parsing-config-providers.md` (new)
- `_bmad-output/implementation-artifacts/sprint-status.yaml` (modified)
- `dotnet/src/Geekmotors.Crucible/Config/`: `ConfigFileLocator`, `ConfigFileRead`, `ConfigFileState`, `ConfigLoader`, `IConfigFileSystem`, `IEnvironment`, `JudgeConfig`, `PhysicalConfigFileSystem`, `ProcessEnvironment`, `ProviderKind`, `Verbosity` (`.cs`, new)
- `dotnet/src/Geekmotors.Crucible/Json/`: `JsoncReader`, `JsonDialect`, `JsonSyntaxException`, `JsonTextParser` (new)
- `dotnet/src/Geekmotors.Crucible/Judging/`: `CoherentReply`, `CoherentReplyReader`, `ExtractReply`, `ExtractReplyReader`, `ExtractType`, `JudgeExchange`, `JudgeReplyText` (new)
- `dotnet/src/Geekmotors.Crucible/Providers/`: `ChatCompletionsClient`, `ChatEndpoint`, `ChatRequestBody`, `ChatResponseReader`, `ExponentialBackoff`, `IJudgeClient`, `IRetryBackoff`, `IWarningSink`, `JudgeKeyReader`, `JudgeRequest`, `PlainHttpRule`, `ProviderAttemptException`, `StandardErrorWarningSink` (new)
- `dotnet/src/Geekmotors.Crucible/Text/EcmaScriptTrim.cs` (new)
- `dotnet/src/Geekmotors.Crucible/Urls/`: `HostKind`, `HostParser`, `HttpUrl`, `HttpUrlParser`, `Ipv4Parser`, `Ipv6Parser`, `PercentEncoding` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Config/`: `ConfigFileLocatorTests`, `ConfigLoaderTests`, `PhysicalConfigFileSystemTests` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Constants/`: `BackoffTimes`, `ConfigPaths`, `ConfigTexts`, `JsonTexts`, `JudgeSettings`, `JudgeTimes`, `TrimTexts`, `UrlCases` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Fakes/`: `FakeConfigFileSystem`, `FakeEnvironment`, `ImmediateBackoff`, `ManualTimeProvider`, `ManualTimer`, `RecordingWarningSink`, `ScriptedHandler`, `ScriptExhaustedException` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Harnesses/`: `ConfigFixtureHarness`, `HttpAnswers`, `JsoncFixtureHarness`, `JudgeReplyFixtureHarness`, `PlainHttpFixtureHarness`, `ProviderFixtureHarness`, `ProviderHarness`, `SpecOperation` (new); `FixtureOutcome`, `SpecCaseRunner`, `SpecialValueDecoder`, `SpecOperations` (modified)
- `dotnet/tests/Geekmotors.Crucible.Tests/Json/`: `JsoncReaderTests`, `JsonTextParserTests`; `Judging/JudgeExchangeTests`; `Providers/ChatCompletionsClientTests`, `Providers/ExponentialBackoffTests`; `Text/EcmaScriptTrimTests`; `Urls/HttpUrlParserTests` (new)
- `dotnet/tests/Geekmotors.Crucible.Tests/Spec/SpecCaseTests.cs` (modified: async)

### Change Log

- 2026-11-15: Code review: 13 patches applied (parser depth limit, IO/cancellation retry classification, UTF-8 response decoding, Uri-unaddressable host as config, ConfigureAwait/CA2007, HttpAnswers to Harnesses, Ipv4 without BigInteger, …); 4 decisions resolved (AC4 narrowed, deviations documented in PORTING.md §5); 3 deferred.
- 2026-11-15: JSONC/strict JSON parser, WHATWG http(s) URL parser, config loading behind file-system/environment boundaries, chat-completions provider (request, response, retries, plain-http warning), judge reply exchange; six spec operations implemented in C#.
