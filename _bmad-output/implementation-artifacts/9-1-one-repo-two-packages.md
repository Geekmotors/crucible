---
baseline_commit: 1d23d5590c12a8fe8a4bfe84f8102d7dd33b9df4
---

# Story 9.1: One repository, two packages

Status: done

## Story

As a maintainer about to add a second language,
I want the npm package in its own folder and a language-neutral landing page,
so that npm and NuGet each show their own README and the docs separate concepts from language APIs.

## Acceptance Criteria

1. **Given** the repository **Then** the npm package moves to `typescript/` (sources, tests, e2e, build config, its `README.md` shown on npm), with `package.json` `repository.directory` set to `typescript` so provenance still verifies (`repository.url` unchanged); CI, `publish.yml`, Makefile, scripts and `spec`/`schemas` references are updated; `schemas/` and `spec/` stay at the root and the package still ships `schemas/*`.
2. **And** the root `README.md` becomes a landing page: what Crucible is, the headline example in TypeScript and C# (C# marked "coming soon"), a "pick your language" table, links to shared docs, licence.
3. **And** `docs/` keeps shared concept pages; the TypeScript API reference moves to `docs/typescript/api.md` and every link to it (README, typescript/README, docs, spec, `spec/surface.json` `docs` fields) is fixed.
4. **And** the gate, `make e2e`, `make conformance`, `make spec-goldens-check` and a dry-run publish all pass from the new layout; no npm API change; `npm pack --dry-run` lists the same 20 files as before the move.

## Tasks / Subtasks

- [x] Record `npm pack --dry-run` file list before moving anything (AC 4)
- [x] `git mv` package files into `typescript/` (AC 1): `src/`, `e2e/`, `scripts/`, `package.json`, `package-lock.json`, `tsconfig.json`, `tsdown.config.ts`, `jest.config.mjs`, `eslint.config.js`, `.prettierignore`, `README.md`, `CHANGELOG.md`
- [x] Schemas into the tarball (AC 1)
  - [x] `typescript/scripts/sync-shared-files.mjs` copies `../schemas/*.md` + `*.schema.json` into `typescript/schemas/` (gitignored); `build` runs it before `tsdown`; `prepack` runs it too
  - [x] `verify-pack.mjs` also proves every packed `schemas/*` file is byte-identical to the root `schemas/` source
  - [x] `LICENSE` stays at root; copied into the package by the same sync step (gitignored) so the tarball still ships it
- [x] Repoint test JSON imports `../../../spec|schemas` → `../../../../spec|schemas` (AC 1)
- [x] `package.json`: `repository.directory: "typescript"`, `prepare: "cd .. && husky"`, `build`/`prepack` schema sync, `format:check` also covers shared `../spec ../schemas ../.github` (AC 1)
- [x] Root `Makefile` keeps every target, pointing into `typescript/` (AC 1, 4)
- [x] Conformance harness: Dockerfile COPY paths + dockerignore; `spec-goldens.sh` repo-root and registry path; prettier from `typescript/node_modules`; regenerate goldens (description text names the new path) (AC 1, 4)
- [x] Workflows: `ci.yml`, `publish.yml` (`working-directory: typescript`, `cache-dependency-path`, tag/version check reads `typescript/package.json`), `conformance.yml` path filters; `dependabot.yml` has no npm entry, so unchanged (AC 1)
- [x] Husky pre-commit runs inside `typescript/` (AC 1)
- [x] `git mv docs/api.md docs/typescript/api.md`; fix its relative links to concept pages (`../x.md`) and all inbound links (AC 3)
- [x] Root `README.md` landing page; `typescript/README.md` = today's README with relative `docs/api.md` links made absolute GitHub URLs to `docs/typescript/api.md` (AC 2, 3)
- [x] Path mentions: `RELEASING.md`, `spec/README.md`, `spec/PORTING.md`, `spec/surface.json` description, `docs/instruction-schema.md`, `docs/judge-models.md`, `docs/judge-validation-runs.md`, `CLAUDE.md` (paths only) (AC 1, 3)
- [x] `npm ci` in `typescript/`; delete root `node_modules`/`dist` after the new install works (AC 4)
- [x] Full gate (AC 4): in `typescript/`: `npm run lint && npm run format:check && npm run typecheck && npm test && npm run build && npm run verify:pack && npx tsc --noEmit -p e2e`; root: `make spec-goldens-check`, `make e2e-offline`; `npm pack --dry-run` diff vs before

### Review Findings

- [ ] [Review][Decision] AC 4 live targets not run: `make e2e` (live smoke), `make conformance` (Docker + real judge) and a dry-run publish need the judge key, Docker and the network. Offline proof only: `make -n`, `docker build --check`, every COPY source exists, `conformance.sh payloads` dry mode, `verify:pack`, pack list. Run them before merge, or rely on the `conformance` workflow (this PR touches `conformance/**` and `Makefile`, so it triggers).
- [ ] [Review][Decision] Harness default judge config stays `typescript/e2e/crucible.config.json` (also the TS live-smoke project config, and the request goldens' `harness-config` input). Move a copy to `conformance/judge.config.json` so the harness is fully language-neutral, or keep the one file?
- [x] [Review][Patch] Conformance harness and cases are language-neutral but lived under `typescript/` → moved to root `conformance/` (harness, `Dockerfile`, `Dockerfile.dockerignore`, `spec-goldens.sh`) and `conformance/cases/{coherent,extract}.json`; Makefile, Dockerfile/dockerignore (context = repo root), `spec-goldens.sh`, `conformance.yml` filters, `conformance-goldens.util.ts`, docs and spec updated; goldens regenerated (descriptions only) [conformance/]
- [x] [Review][Patch] `verify:pack` threw a raw ENOENT when a packed shared file had no root counterpart, did not notice a root schema missing from the tarball, and resolved paths from cwd [typescript/scripts/verify-pack.mjs]
- [x] [Review][Patch] `sync-shared-files.mjs` deleted `typescript/schemas/` before failing on a missing `../schemas` or `../LICENSE` → checks sources first [typescript/scripts/sync-shared-files.mjs]
- [x] [Review][Patch] `make spec-goldens` without `typescript/node_modules` failed with an unreadable error → clear usage error [conformance/spec-goldens.sh]
- [x] [Review][Patch] `format:check` stopped covering the conformance cases after the move → adds `../conformance` [typescript/package.json]
- [x] [Review][Patch] Root README: TypeScript example lacked its import; layout table lacked `conformance/` [README.md]
- [x] [Review][Defer] `conformance.yml` path filters omit `typescript/src/providers/**` (request builder) — pre-existing; request goldens cover parity on every PR
- [x] [Review][Defer] `prepare: "cd .. && husky"` assumes the parent is the git root; CI does not set `HUSKY=0` — husky 9 exits 0 without `.git`; low
- [x] [Review][Defer] Pre-commit hook does not run `make spec-goldens-check` — pre-existing; CI catches it

## Dev Notes

### What stays at the root and why

- `schemas/`, `spec/` — shared, language-neutral contracts (Epic 9 intro: the .NET port runs the same fixtures).
- `docs/` — shared concept pages; only the TS API reference moves to `docs/typescript/`.
- `.github/`, `.husky/`, `_bmad*`, `.claude/`, `CLAUDE.md`, `RELEASING.md`, `Makefile`, `LICENSE`, `.gitignore`, `.prettierrc.json` — repo-wide. Prettier resolves `.prettierrc.json` by walking up, so one root config serves `typescript/` and the shared JSON/YAML. `.prettierignore` is read from the cwd only, so it moves to `typescript/`.
- `.env.dev.local` — never opened or moved; Makefile/RELEASING keep telling people to source it from the root.

### Current state that must be preserved

- `SchemaSource` (`src/core/schema-source.ts`) walks up from its own file to the first `package.json` and reads `<root>/schemas/<name>.md`. After the move that root is `typescript/` (dev, e2e against `dist/`) or `node_modules/crucible-ai/` (installed). So `typescript/schemas/` must exist whenever the built package runs → the sync step runs in `build`. No source change needed.
- `files` in `package.json` stays `dist`, `schemas/*.md`, `schemas/*.schema.json`, `CHANGELOG.md` (npm `files` cannot reach `../`). README, LICENSE, package.json are always included by npm when present in the package dir.
- `publish.yml` runs `npm ci --ignore-scripts` then `npm run build` then `npm stage publish --ignore-scripts`; the sync must therefore be inside the `build` script itself, not a `pre*` hook alone.
- `scripts/verify-pack.mjs` runs `npm pack --dry-run --json --ignore-scripts`; it is the CI proof the tarball holds exactly the allow-listed files.
- `spec-goldens.sh` check also enforces every `spec/**/*.json` is registered in `src/core/test/spec-fixtures.util.ts` by grepping for `'../../../<file>'` → becomes `'../../../../<file>'`.
- Generated goldens (`spec/payload/*-conformance.json`, `spec/provider/request-conformance.json`) embed the generator path in `description`; regenerate with `make spec-goldens` (no network: `conformance.sh request` only builds the request).
- e2e imports `crucible-ai` via package self-reference and resolves jest via `../node_modules` relative to `e2e/` → unchanged inside `typescript/`.

### Constraints

- Mechanical restructure: zero behaviour change in `src/`; no new `src/` behaviour → TDD rule not triggered (scaffold/config/scripts exempt). Tests move with the code (`git mv`).
- Never read `.env*`. No network judge calls, no Docker.
- Release tag scheme unchanged (`v*` for npm); Epic 9 later adds `dotnet-v*`, which `v*` does not match.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Epic 9, Story 9.1]
- [Source: RELEASING.md] provenance requires `repository.url` unchanged
- [Source: src/core/schema-source.ts] package-root lookup

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- `npm pack --dry-run` before the move: 20 files (CHANGELOG.md, LICENSE, README.md, 13 × dist/*, package.json, schemas/coherent.md, schemas/crucible.config.schema.json, schemas/extract.md). After: the same 20 paths, same dist chunk hashes.
- Prettier treated an unanchored `schemas/` in `typescript/.prettierignore` as also matching `../schemas`; anchored it to `/schemas/` so the shared config JSON Schema is still format-checked.

### Completion Notes List

- Mechanical restructure only; no `src/` behaviour change, so no new tests (TDD rule applies to behaviour). Unit suite 40 suites / 1477 tests green; offline e2e green.
- Schemas + LICENSE reach the tarball via `typescript/scripts/sync-shared-files.mjs` (run inside `build` and as `prepack`), copies gitignored. `verify-pack.mjs` now fails if a packed `schemas/*` or `LICENSE` differs from the root copy, and its required-file list still fails when they are missing (both proven by hand).
- `SchemaSource` unchanged: it finds `typescript/package.json` (dev/e2e) or the installed package root, then `schemas/`.
- Root Makefile kept (all targets, run from root), paths via `TS_DIR`/`CONFORMANCE_HARNESS`. `.env.dev.local` stays at root, never opened.
- Generated goldens regenerated: only their `description` text changed (generator path now `typescript/e2e/...`).
- `format`/`format:check` also cover `../spec ../schemas ../.github ../.prettierrc.json`, preserving what the root-level prettier run checked before.
- `homepage` now points at `tree/main/typescript#readme`; `CHANGELOG.md` gained an Unreleased note about the move.
- Gate: lint, format:check, typecheck, test, build, verify:pack, `tsc -p e2e` (in `typescript/`); `make spec-goldens-check`, `make e2e-offline` (root) all pass. `make e2e`/`make conformance` not run (live judge / Docker); their commands were dry-run (`make -n`) and every Docker COPY source checked to exist.

### File List

- Moved (`git mv`) to `typescript/`: `src/**`, `e2e/**`, `scripts/verify-pack.mjs`, `package.json`, `package-lock.json`, `tsconfig.json`, `tsdown.config.ts`, `jest.config.mjs`, `eslint.config.js`, `.prettierignore`, `README.md`, `CHANGELOG.md`
- Moved: `docs/api.md` → `docs/typescript/api.md`
- New: `README.md` (landing page), `typescript/scripts/sync-shared-files.mjs`
- Moved (review): `typescript/e2e/ext/conformance/*` → `conformance/`; `typescript/e2e/ext/fixtures/{coherent,extract}-conformance.json` → `conformance/cases/{coherent,extract}.json`; modified `typescript/src/core/test/conformance-goldens.util.ts`
- Modified: `typescript/package.json`, `typescript/.prettierignore`, `typescript/README.md`, `typescript/CHANGELOG.md`, `typescript/scripts/verify-pack.mjs`, `typescript/src/core/test/spec-fixtures.util.ts`, `typescript/src/core/test/spec-surface.util.ts`, `typescript/src/core/test/public-surface.util.ts`, `typescript/src/core/test/config-schema-harness.util.ts`, `typescript/e2e/ext/conformance/Dockerfile`, `typescript/e2e/ext/conformance/Dockerfile.dockerignore`, `typescript/e2e/ext/conformance/spec-goldens.sh`, `.github/workflows/ci.yml`, `.github/workflows/publish.yml`, `.github/workflows/conformance.yml`, `.gitignore`, `.husky/pre-commit`, `Makefile`, `RELEASING.md`, `CLAUDE.md`, `docs/README.md`, `docs/configuration.md`, `docs/getting-started.md`, `docs/gotchas.md`, `docs/instruction-schema.md`, `docs/judge-models.md`, `docs/judge-validation-runs.md`, `docs/runs-and-threshold.md`, `docs/typescript/api.md`, `spec/README.md`, `spec/PORTING.md`, `spec/surface.json`, `spec/payload/coherent-conformance.json`, `spec/payload/extract-conformance.json`, `spec/provider/request-conformance.json`, `_bmad-output/implementation-artifacts/sprint-status.yaml`

## Change Log

- 2026-11-15: Story 9.1 implemented: npm package moved to `typescript/`, root landing page, TS API reference at `docs/typescript/api.md`, CI/Makefile/harness repointed; status → review.
- 2026-11-15: Code review: conformance harness and cases moved to root `conformance/`; verify-pack/sync/spec-goldens hardening; 2 decisions open (live AC 4 runs, harness judge config location).
