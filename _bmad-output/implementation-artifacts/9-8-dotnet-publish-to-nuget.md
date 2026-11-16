---
baseline_commit: f87d92f5084026b543cdf0172efc865054db5998
---

# Story 9.8: Publish to NuGet

Status: done

## Story

As the maintainer releasing the C# port,
I want `Geekmotors.Crucible` and `Geekmotors.Crucible.Xunit` packed with full metadata, checked, and published by a tag-driven workflow with NuGet trusted publishing,
so that .NET users can `dotnet add package` a verified, source-linked release without anyone holding an API key.

## Acceptance Criteria

1. **Packaging** (`dotnet/Directory.Build.props` + the two `src/` csproj files): `PackageId`s `Geekmotors.Crucible` and `Geekmotors.Crucible.Xunit`; ONE `<Version>` (0.1.0) in `dotnet/Directory.Build.props`; `Authors` Geekmotors; per-package `Description` and `PackageTags`; `PackageLicenseExpression` MIT; `PackageReadmeFile` README.md (both packages pack `dotnet/README.md`, which documents both); `PackageProjectUrl`/`RepositoryUrl` `https://github.com/Geekmotors/crucible-ai`, `RepositoryType` git; SourceLink via the .NET 8+ SDK built-in support (no `Microsoft.SourceLink.GitHub` package), `PublishRepositoryUrl`, `EmbedUntrackedSources`; `IncludeSymbols` + `SymbolPackageFormat` snupkg; deterministic (already) and `ContinuousIntegrationBuild` on GitHub Actions (already). Test, fixture and live projects stay `IsPackable=false` and are never packed.
2. **Exact pin**: the `Geekmotors.Crucible.Xunit` nuspec depends on `Geekmotors.Crucible` `[X.Y.Z]` (exactly its own version, it uses engine internals), produced from the existing `ProjectReference`; proven by `dotnet pack` and reading the `.nuspec` inside the `.nupkg`. The Xunit package keeps `build/` and `buildTransitive/` targets.
3. **Pack check** (`dotnet/scripts/verify-pack.sh <dir>`, `make dotnet-pack-check`): fails unless both `.nupkg` and both `.snupkg` exist at the `<Version>`; each `.nupkg` holds only the expected files (`lib/net8.0` + `lib/net10.0` DLL and XML docs, `README.md`, `build/` + `buildTransitive/` targets for Xunit, NuGet's own metadata files); the engine DLL embeds `schemas/coherent.md` and `schemas/extract.md` and declares the spec (AC 6); the exact-version dependency; licence expression MIT; repository metadata with the HEAD commit; readme declared. Runs in CI's dotnet job.
4. **`.github/workflows/publish-dotnet.yml`**: on `push` tags `dotnet-v*`; workflow-level `permissions: contents: read`; job `verify` (checkout full history, setup-dotnet 8 + 10, tag == `dotnet-v` + `<Version>`, tagged commit on `main`, locked restore, build `-warnaserror`, test, format check, Release pack, pack check, upload the packages as an artifact); job `publish` (needs verify, guarded by `github.repository == 'Geekmotors/crucible-ai'`, `environment: nuget`, `permissions: id-token: write, contents: read`) downloads the verified packages, `NuGet/login` with `user: Jackula`, `dotnet nuget push` with the short-lived key and `--skip-duplicate`. All actions pinned by SHA with a version comment. No secrets.
5. **Docs**: `RELEASING.md` gains a .NET release section; new `dotnet/CHANGELOG.md` with `## 0.1.0`; `dotnet/README.md` no longer says "until it is published"; root `README.md` states 0.1.0 on NuGet; `spec/PORTING.md` §4 C# row declares the spec by tag (no embedded commit hash) and drops the "ships with its first NuGet release" note.
6. **Spec declaration** (PORTING §4: "one machine-readable file it ships ... or an assembly attribute"): `Geekmotors.Crucible.dll` carries `[AssemblyMetadata("CrucibleSpec", "dotnet-v<Version>")]`, i.e. the `spec/` at the release tag; README and PORTING say so.
7. Release itself (tag `dotnet-v0.1.0`, approve `nuget`, verify on nuget.org) is the owner's manual step per `RELEASING.md`; this story does not tag, push or publish.

## Tasks / Subtasks

- [x] Packaging metadata + version + SourceLink + symbols (AC 1)
- [x] Exact-version pin target in the Xunit csproj; prove in nuspec (AC 2)
- [x] Spec metadata attribute (AC 6)
- [x] `dotnet/scripts/verify-pack.sh`, Make targets, CI dotnet job pack + check, `.gitignore` for `dotnet/artifacts/` (AC 3)
- [x] `publish-dotnet.yml` (AC 4)
- [x] Docs: RELEASING, dotnet CHANGELOG, dotnet README, root README, PORTING §4; deferred-work items closed (AC 5)
- [x] Gates: dotnet restore/build/test/format/pack + pack check; TS lint/format/test; `make spec-goldens-check`

### Review Findings

- [ ] [Review][Decision] Package-ID squatting window — `Geekmotors.` prefix reservation is optional, and `main` advertises `dotnet add package Geekmotors.Crucible*` before the IDs exist. Recommend: request ID prefix reservation for `Geekmotors.` on nuget.org now, and tag `dotnet-v0.1.0` right after the merge.
- [ ] [Review][Decision] Packages owned by a personal nuget.org account (`Jackula`) — single point of failure. Recommend: a nuget.org organization `Geekmotors` as package and policy owner with a second human owner (can follow the first release).
- [ ] [Review][Decision] No build provenance attestation for the `.nupkg` (npm side has provenance). Recommend: a separate `attest` job (`actions/attest-build-provenance`, `id-token`/`attestations: write`) that only downloads the verified artifact, so verify stays credential-free; follow-up story.
- [ ] [Review][Decision] Tested binaries are Debug, shipped binaries are a separate Release build. Recommend: build/test with `-c Release` and `dotnet pack --no-build` in CI and publish once the adapter child-process fixture is confirmed configuration-agnostic; follow-up.
- [x] [Review][Patch] Extra packages in the pack folder would be pushed by the `*.nupkg` glob [dotnet/scripts/verify-pack.sh, .github/workflows/publish-dotnet.yml] — folder must hold exactly the four release files; publish pushes the two named packages, engine first.
- [x] [Review][Patch] No integrity link between verify and publish beyond the artifact digest [.github/workflows/publish-dotnet.yml] — verify records SHA-256s as a job output; publish checks them (`--strict`) and rejects extra files.
- [x] [Review][Patch] First tag run could auto-create an unprotected `nuget` environment [RELEASING.md] — environment with required reviewer is now a precondition before the first tag.
- [x] [Review][Patch] `--skip-duplicate` + re-tag can ship a mismatched engine/adapter pair [RELEASING.md] — never re-tag a version once anything is on nuget.org; re-run failed jobs only.
- [x] [Review][Patch] `VERSION` from sed can be empty/multi-line; version and schema used as regex [dotnet/scripts/verify-pack.sh] — validated charset, `grep -F`, escaped pattern.
- [x] [Review][Patch] `CrucibleSpec` and version checked independently [dotnet/scripts/verify-pack.sh] — now matches the attribute blob `CrucibleSpec<len>dotnet-v<Version>`.
- [x] [Review][Patch] Missing DLL / nuspec / git checkout aborted under `set -e` with no named problem [dotnet/scripts/verify-pack.sh] — each now reported via `fail` or a clear exit 2.
- [x] [Review][Patch] Repository attribute check depended on attribute order [dotnet/scripts/verify-pack.sh] — each attribute checked separately.
- [x] [Review][Patch] `.snupkg` contents and corrupt zips not checked [dotnet/scripts/verify-pack.sh] — `unzip -tq` and expected-list check for symbols.
- [x] [Review][Patch] EXIT trap skipped on signals [dotnet/scripts/verify-pack.sh] — HUP/INT/TERM exit through the trap.
- [x] [Review][Patch] Artifact retention 7 days shorter than environment approval window; re-run-all upload name clash [.github/workflows/publish-dotnet.yml] — retention 30, `overwrite: true`.
- [x] [Review][Patch] No CHANGELOG gate for the tagged version [.github/workflows/publish-dotnet.yml] — verify fails without `## <Version>`.
- [x] [Review][Patch] `msbuild -getProperty` output not normalised [.github/workflows/publish-dotnet.yml] — last line, CR stripped.
- [x] [Review][Patch] Exact-pin target pinned any project reference, not idempotent, silent if the private target changes [dotnet/src/Geekmotors.Crucible.Xunit/Geekmotors.Crucible.Xunit.csproj] — scoped to the engine, skips ranges, `Error` when no pinned engine reference reaches the nuspec.
- [x] [Review][Patch] Root README no longer stated 0.1.0 (AC 5) [README.md] — restored.
- [x] [Review][Defer] SourceLink URL hard-coded to `Geekmotors/crucible-ai`, so fork CI fails the pack check [dotnet/scripts/verify-pack.sh] — deferred; message now names the origin remote.
- [x] [Review][Defer] `CrucibleSpec` is stamped on every build, not only tag builds [dotnet/src/Geekmotors.Crucible/Geekmotors.Crucible.csproj] — deferred, it is a declaration by design.
- [x] [Review][Defer] Local `make dotnet-pack-check` passes on a dirty tree [dotnet/scripts/verify-pack.sh] — deferred, CI trees are clean.
- [x] [Review][Defer] Symbols on a `--skip-duplicate` re-run after a nupkg-only partial push are unverified [.github/workflows/publish-dotnet.yml] — deferred; RELEASING step 6 checks Symbols on nuget.org.
- [x] [Review][Defer] RELEASING exports the whole `.env.dev.local` for `make dotnet-live`/`conformance` [RELEASING.md] — deferred, same as the npm section.

## Dev Notes

### Current state (read before changing)

- `dotnet/Directory.Build.props`: LangVersion, nullable, warnings as errors, `Deterministic`, `ContinuousIntegrationBuild` when `GITHUB_ACTIONS`, lock files, XML docs, `RepositoryRoot`/`SpecRoot`. Imported by every project (tests too), so package metadata placed here is harmless for non-packable projects.
- `src/Geekmotors.Crucible/Geekmotors.Crucible.csproj`: net8.0;net10.0, `IsPackable` true, embeds `schemas/*.md` as `schemas/<name>` resources, `InternalsVisibleTo` tests + `Geekmotors.Crucible.Xunit` (hence the exact pin).
- `src/Geekmotors.Crucible.Xunit/Geekmotors.Crucible.Xunit.csproj`: depends on `xunit.v3.extensibility.core`, `ProjectReference` to the engine, packs `build/Geekmotors.Crucible.Xunit.targets` into `build/` and `buildTransitive/`. Test/fixture/live projects import that targets file directly; must keep working.
- Baseline `dotnet pack` today: both packages at 1.0.0, "missing a readme" warning, dependency `>= 1.0.0`, no symbols.
- `ci.yml` dotnet job: locked restore, build -warnaserror, test, format. `publish.yml` (npm) is the template for tag/version and on-main checks (`fetch-depth: 0`, `git merge-base --is-ancestor "$GITHUB_SHA" origin/main`).
- Pinned actions in use: checkout `3d3c42e5aac5ba805825da76410c181273ba90b1` v7.0.1, setup-dotnet `0f4a354dd5547a28c70b7853dbd95d5b67886e73` v6.1.0. Dependabot watches github-actions and nuget.

### Technical decisions

- **Exact pin**: NuGet pack turns a `ProjectReference` into `_ProjectReferencesWithVersions` items whose `ProjectVersion` metadata becomes the dependency range; a target before `GenerateNuspec` (and `_GetProjectReferenceVersions` output) rewrites it to `[%(ProjectVersion)]`. Scoped to the Xunit csproj.
- **SourceLink**: .NET 8+ SDK bundles `Microsoft.SourceLink.GitHub`; with `PublishRepositoryUrl` the nuspec gets `<repository ... commit="..."/>`.
- **Build once, publish what was verified**: verify packs and uploads the `.nupkg`/`.snupkg` (upload-artifact v7.0.2 `cf430e030ddbb5b0abf93d22962f4752f3646cd9`); publish downloads them (download-artifact v8.0.2 `9000827ccba6bdab643e8b6fd33ac0654aef8333`) and never builds. The publish job holds `id-token: write` and runs no repository code beyond `dotnet nuget push`.
- **NuGet/login** v1.2.0 `8d196754b4036150537f80ac539e15c2f1028841` (tag v1 → same commit). action.yml: input `user` (required), optional `token-service-url` (default `https://www.nuget.org/api/v2/token`), `audience` (default `https://www.nuget.org`); output `NUGET_API_KEY`. Push: `dotnet nuget push <pkg>.nupkg --api-key ... --source https://api.nuget.org/v3/index.json --skip-duplicate`; the matching `.snupkg` beside it is pushed automatically.
- Trusted publishing on nuget.org: owner `Geekmotors`, repo `crucible-ai`, workflow `publish-dotnet.yml`, environment `nuget`, user `Jackula` (owner configured).

### Testing standards

Build/config/workflow/docs only, no `src/` behaviour change, so the TDD rule's scaffold/config exemption applies. Proof is the pack check script (red first against the baseline pack, green after) plus the existing suites.

### References

- `_bmad-output/planning-artifacts/epics.md` Epic 9, Story 9.8
- `_bmad-output/implementation-artifacts/9-6-dotnet-xunit-adapter.md`, `9-7-dotnet-ichatclient-and-docs.md`, `deferred-work.md` (9.6 exact pin; 9.7 release-prep docs)
- `spec/PORTING.md` §4; `RELEASING.md`; `.github/workflows/publish.yml`, `ci.yml`
- NuGet/login README and action.yml (via `gh api repos/NuGet/login/contents/...`)

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- Red: `verify-pack.sh` against the baseline pack (1.0.0, no README/symbols, `>= ` dependency) failed; after packaging changes the repository check failed once because SourceLink adds a `branch` attribute to `<repository>` (check now matches type/url/commit, not the whole element). Negative test (unpinned dependency + extra file injected into a copy of the packages) fails with both problems named.
- CI-mode pack (`-p:ContinuousIntegrationBuild=true`) passes the check; PDB SourceLink maps `/_/*` to `raw.githubusercontent.com/Geekmotors/crucible-ai/<commit>/*`.

### Completion Notes List

- Ultimate context engine analysis completed - comprehensive developer guide created
- Packaging: `<Version>0.1.0</Version>` + shared metadata (Authors, MIT expression, project/repository URL, git, `PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols` + snupkg, `PackageReadmeFile`, release notes link) in `dotnet/Directory.Build.props`; per-package `PackageId`, `Description`, `PackageTags` and the packed `dotnet/README.md` in each csproj. SourceLink comes from the .NET SDK (no package). No package icon.
- Exact pin: target `PinEngineToExactVersion` in the Xunit csproj rewrites `_ProjectReferencesWithVersions` `ProjectVersion` to `[%(ProjectVersion)]` after `_GetProjectReferenceVersions`; nuspec shows `<dependency id="Geekmotors.Crucible" version="[0.1.0]" .../>` for net8.0 and net10.0.
- Spec declaration: `[AssemblyMetadata("CrucibleSpec", "dotnet-v$(Version)")]` on the engine assembly (PORTING §4 allows an assembly attribute).
- Pack check `dotnet/scripts/verify-pack.sh`, `make dotnet-pack` / `make dotnet-pack-check` (output `dotnet/artifacts/packages/`, gitignored), CI dotnet job packs and checks.
- `.github/workflows/publish-dotnet.yml`: verify builds once, checks, uploads `nuget-packages`; publish (env `nuget`, OIDC) downloads exactly those, `NuGet/login` (user `Jackula`), `dotnet nuget push packages/*.nupkg --skip-duplicate`; API key passed via `env`, not inlined in the script.
- Docs: RELEASING `.NET packages` section; `dotnet/CHANGELOG.md` 0.1.0; dotnet README (no "until it is published", exact-pin note, spec declaration, pack check); root README (NuGet links, install lines); PORTING §4 C# row by release tag, no commit hash, removed "ships with its first NuGet release". Deferred-work 9.6 pin and 9.7 release-prep items marked resolved.
- TDD: no `src/` behaviour changed (build config, script, workflow, docs: the scaffold/config exemption); the pack check was run red before the packaging change and green after.
- Gates: dotnet restore --locked-mode, build -warnaserror, test (3258 total, 0 failed, 80 skipped), format, `make dotnet-pack-check`; TS lint, format:check (workflow YAML), test (1477 passed); `make spec-goldens-check`.
- Not done here (owner): tag `dotnet-v0.1.0`, approve `nuget`, verify on nuget.org — so the epic AC "0.1.0 released" is pending that manual step.

### File List

- `.github/workflows/publish-dotnet.yml` (new)
- `.github/workflows/ci.yml`
- `.gitignore`
- `Makefile`
- `README.md`
- `RELEASING.md`
- `spec/PORTING.md`
- `dotnet/Directory.Build.props`
- `dotnet/src/Geekmotors.Crucible/Geekmotors.Crucible.csproj`
- `dotnet/src/Geekmotors.Crucible.Xunit/Geekmotors.Crucible.Xunit.csproj`
- `dotnet/scripts/verify-pack.sh` (new)
- `dotnet/CHANGELOG.md` (new)
- `dotnet/README.md`
- `_bmad-output/implementation-artifacts/9-8-dotnet-publish-to-nuget.md` (new)
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/implementation-artifacts/deferred-work.md`

## Change Log

- 2026-11-15: NuGet packaging (metadata, SourceLink, symbols, README, exact engine pin, spec assembly metadata), pack check in CI, `publish-dotnet.yml` with trusted publishing, .NET release docs and changelog.

### Review decisions (2026-11-15)

- Owner actions, surfaced in the PR (not code): reserve the `Geekmotors.` ID prefix on nuget.org and tag soon after merge (squatting window); consider a nuget.org organization `Geekmotors` with a second owner.
- Follow-up stories (deferred-work): build provenance attestation for the NuGet packages (separate attest job on the verified artifact); test and pack the same Release build (`dotnet pack --no-build`).
