# Releasing crucible-ai

This repository releases two things independently: the npm package `crucible-ai` (this page, up to [.NET packages](#net-packages)) and the NuGet packages `Geekmotors.Crucible` and `Geekmotors.Crucible.Xunit` ([.NET packages](#net-packages)). Their versions are independent and their tags differ: `v*` for npm, `dotnet-v*` for NuGet.

The npm package lives in `typescript/`. Releases are staged by `.github/workflows/publish.yml` when a `v*` tag is pushed, and go live only when a maintainer approves them on npmjs.com with 2FA. Nobody runs `npm publish` by hand: `publishConfig.provenance` is `true`, so a local publish fails on purpose (provenance can only be generated in CI).

## What the workflow does

1. **verify** (Node 22 and 24, no publish credentials)
   - fails unless the tag is exactly `v` + the `typescript/package.json` version
   - fails unless the tagged commit is on `main`
   - in `typescript/`: `npm ci`, lint, format check, typecheck, unit tests, build (the build copies the root `schemas/` and `LICENSE` into the package)
   - `npm run verify:pack`: the tarball may contain only `dist/` (including the `crucible-ai` CLI, `dist/cli.js`), `schemas/*.md`, `schemas/crucible.config.schema.json`, `README.md`, `CHANGELOG.md`, `LICENSE` and `package.json` (all from `typescript/`), and the packed `schemas/*` and `LICENSE` must match the root copies byte for byte
2. **publish** (needs verify; GitHub environment `npm`; the only job with `id-token: write`)
   - installs npm 11.21.2 (staged publishing needs npm 11.15.0 or later and Node 22.14.0 or later)
   - `npm ci --ignore-scripts`, build, pack check again
   - `npm stage publish --provenance --access public --ignore-scripts`: the version goes to the staged queue, not live
3. **approve** (you, by hand): on npmjs.com open **crucible-ai → Staged Packages**, check the name, version and contents, and click **Approve** (2FA prompt). Or run `npm stage approve <stage-id>`. Nothing is public until you approve. Provenance is generated for staged versions the same as for direct publishes.

Authentication: npm tries Trusted Publishing (OIDC) first. If no trusted publisher matches, it falls back to the `NPM_TOKEN` secret, which reaches npm as `NODE_AUTH_TOKEN`. Every token type can stage, and staging never asks for 2FA, so a stage-only token is enough. Approval always needs a human with 2FA; neither the token nor OIDC can approve. The workflow uses no other secret.

## One-time setup

npm only lets you configure a trusted publisher on a package that already exists, so the first release is staged with a token and later releases use OIDC. Staging a package that does not exist yet creates it and publishes a public placeholder version `0.0.0-stage`; the real version stays private until you approve it.

### 1. First publish (token)

1. Sign in to npmjs.com as the account that will own `crucible-ai`, with 2FA turned on.
2. Go to **Access Tokens → Generate New Token → Granular Access Token**:
   - Permissions: **Read and write (stage only)**, packages: **All packages**. The package does not exist yet, so you cannot pick it. A stage-only token cannot put a version live, even if it leaks.
   - Expiry: as short as you like. It is only needed until Trusted Publishing is set up.
3. In GitHub, go to **Geekmotors/crucible-ai → Settings → Secrets and variables → Actions → New repository secret**. Name it `NPM_TOKEN` and paste the token. A secret on the `npm` environment also works.
4. Optional: in **Settings → Environments → `npm`** (it appears after the first workflow run, or you can create it now), add yourself as a **Required reviewer**. Every publish then waits for your approval.
5. Optional, recommended: **Settings → Rules → Rulesets → New tag ruleset** targeting `v*`, restricting creation, update and deletion to yourself, so only you can trigger a publish.
6. Cut the release as described in [Every release](#every-release) (tag `v0.1.0`).

### 2. Switch to Trusted Publishing (after the package exists)

1. On npmjs.com, open **crucible-ai → Settings → Trusted Publisher → GitHub Actions** and enter:
   - Organization or user: `Geekmotors`
   - Repository: `crucible-ai`
   - Workflow filename: `publish.yml`
   - Environment: `npm`. You can leave it blank, but if you fill it in it must match the workflow's `environment: npm`.
2. Delete the `NPM_TOKEN` GitHub secret and revoke the token on npmjs.com.
3. Recommended: under **Settings → Publishing access**, choose **Require two-factor authentication and disallow tokens**. Trusted publishing still works, and a leaked token cannot publish.
4. Optional: in the trusted publisher settings, restrict it to staging only, so `npm publish` from the workflow is rejected and only `npm stage publish` is accepted. The workflow already only stages.

Check these before tagging, or the publish fails with an auth or provenance error:

- `typescript/package.json` `repository.url` must stay `git+https://github.com/Geekmotors/crucible-ai.git`, with `repository.directory` `typescript`. Provenance checks the URL against the repository the workflow ran in.
- If you rename the workflow file or the environment, update the trusted publisher to match.
- The repository must stay public. npm provenance is only generated for public source repositories.

## Every release

1. Merge everything for the release into `main`. The npm README (`typescript/README.md`) links to `blob/main/docs/...`, so the docs have to be on `main` before anyone reads the npm page.
2. Check the judge and the e2e suite on the `main` commit you are releasing from (the version bump in step 3 changes no judged input). `.env.dev.local` must set `CRUCIBLE_JUDGE_KEY` to your OpenRouter key.
   ```sh
   set -a; . ./.env.dev.local; set +a   # from the repository root, like the make targets
   make conformance   # needs Docker; judges every conformance case against real OpenRouter
   make e2e           # stub-judge suites plus the one live OpenRouter smoke (2 judge calls)
   ```
   A passing `make conformance` is required before tagging: model behaviour is tested only there, not in `make e2e` or the unit tests. A green **Actions → conformance** run on that commit counts instead (start it with **Run workflow** on `main` if no push triggered it).
3. Bump the version in a PR and merge it to `main`. In the same PR, rename `## Unreleased` in `typescript/CHANGELOG.md` to the new version (the changelog ships in the tarball):
   ```sh
   cd typescript && npm version 0.2.0 --no-git-tag-version   # edits typescript/package.json and package-lock.json
   ```
4. Tag the merged commit on `main` and push only that tag:
   ```sh
   git switch main && git pull --ff-only
   git tag v0.2.0
   git push origin v0.2.0
   ```
   `git push --tags` also works, but it pushes every local tag, and any of them matching `v*` would start its own publish run. Those runs would fail the version check, but they are noise.
5. Watch **Actions → publish**. Approve the `npm` environment if you set up a reviewer.
6. Approve the staged version on npmjs.com (**crucible-ai → Staged Packages → Approve**, 2FA) or with `npm stage approve <stage-id>`.
7. Check the result:
   - `npm view crucible-ai@0.2.0`
   - On npmjs.com, the version page shows a **Provenance** badge that links to the workflow run.
   - In a consumer project: `npm audit signatures`.

## If something goes wrong

- **Tag/version mismatch, or tag not on main.** Nothing was published. Delete the tag (`git push --delete origin v0.2.0 && git tag -d v0.2.0`), fix the cause, and tag again.
- **A gate fails in verify.** Nothing was published. Fix the problem on `main`, then move the tag the same way.
- **The publish step fails with an auth error (E401/E403/ENEEDAUTH).** Either no trusted publisher matches (wrong repo, workflow file or environment) and no `NPM_TOKEN` is set, or the token has expired or lacks write (stage) permission. Fix the setting and re-run the failed job.
- **You need to pull back a release.** npm versions are immutable, so a version number can never be reused. Publish a fixed patch version. Use `npm deprecate crucible-ai@<version> "<reason>"` for the bad one, and `npm unpublish` only within npm's 72-hour window.

## .NET packages

`Geekmotors.Crucible` and `Geekmotors.Crucible.Xunit` live in `dotnet/` and are always released together at one version: `<Version>` in `dotnet/Directory.Build.props`. `Geekmotors.Crucible.Xunit` uses engine internals, so its package depends on exactly the same `Geekmotors.Crucible` version (`[X.Y.Z]`). Releases are published by `.github/workflows/publish-dotnet.yml` when a `dotnet-v*` tag is pushed. Nobody runs `dotnet nuget push` by hand, and there is no NuGet API key anywhere: the workflow logs in with NuGet trusted publishing and gets a key that lives for one hour.

### What the workflow does

1. **verify** (no publish credentials)
   - fails unless the tag is exactly `dotnet-v` + `<Version>`
   - fails unless `dotnet/CHANGELOG.md` has a `## <Version>` section
   - fails unless the tagged commit is on `main`
   - in `dotnet/`: locked restore, `dotnet build -warnaserror`, `dotnet test` (both target frameworks, all `spec/` fixtures), `dotnet format --verify-no-changes`, `dotnet pack -c Release`
   - `scripts/verify-pack.sh` (also run by CI on every PR, and locally by `make dotnet-pack-check`): the folder holds exactly both `.nupkg` and both `.snupkg` at `<Version>` and nothing else; each `.snupkg` holds only the two PDBs; each package holds only `lib/net8.0` and `lib/net10.0` (DLL and XML docs), `README.md` (byte-equal to `dotnet/README.md`) and, for the adapter, the `build/` and `buildTransitive/` targets; the engine DLL embeds `schemas/coherent.md` and `schemas/extract.md` and declares `CrucibleSpec` `dotnet-v<Version>`; the adapter depends on `Geekmotors.Crucible [<Version>]` for both frameworks; licence `MIT`, readme, project URL and repository URL with the tagged commit are set; each PDB has SourceLink to that commit
   - records the SHA-256 of each checked package as a job output and uploads the packages as the `nuget-packages` artifact (kept 30 days)
2. **publish** (needs verify; GitHub environment `nuget`; the only job with `id-token: write`; runs only in `Geekmotors/crucible-ai`)
   - downloads the packages verify checked and fails unless their SHA-256s match verify's and the artifact holds nothing else (nothing is rebuilt, no repository code runs)
   - `NuGet/login` with user `Jackula` exchanges the GitHub OIDC token for a short-lived NuGet API key
   - `dotnet nuget push --skip-duplicate` to nuget.org, `Geekmotors.Crucible` first, then `Geekmotors.Crucible.Xunit` (it depends on the engine); each `.snupkg` beside its `.nupkg` goes to the symbol server automatically. `--skip-duplicate` makes a re-run of this job after a partial push safe, because the re-run pushes the same files.

### One-time setup

1. On nuget.org, signed in as `Jackula`: **Trusted Publishing** policy for owner `Geekmotors`, repository `crucible-ai`, workflow file `publish-dotnet.yml`, environment `nuget` (done). If you rename the workflow file or the environment, update the policy to match.
2. Package IDs: the first push creates `Geekmotors.Crucible` and `Geekmotors.Crucible.Xunit` under the account the policy belongs to. Optional: reserve the `Geekmotors.` prefix on nuget.org (ID prefix reservation) so nobody else can publish under it.
3. Before the first `dotnet-v*` tag is pushed, in GitHub, **Settings → Environments → New environment `nuget`**: add yourself as a **Required reviewer**, and under deployment branches and tags allow only tags matching `dotnet-v*`. Every publish then waits for your approval. Do not let the first run create the environment: an environment GitHub creates on the fly has no protection rules, so that run would publish without approval.
4. Recommended: a tag ruleset for `dotnet-v*` (like the one for `v*`), so only you can create, move or delete release tags.

### Every .NET release

1. Merge everything for the release into `main`. The NuGet README (`dotnet/README.md`) links to `blob/main/...`, so the docs must be on `main` before anyone reads the NuGet page.
2. Check the release on the `main` commit you are releasing from:
   ```sh
   make dotnet-test         # build -warnaserror and every test, both frameworks
   make dotnet-format
   make dotnet-pack-check   # packs into dotnet/artifacts/packages/ and runs scripts/verify-pack.sh
   set -a; . ./.env.dev.local; set +a
   make dotnet-live         # the opt-in live smoke against OpenRouter (CRUCIBLE_JUDGE_KEY)
   make conformance         # needs Docker; required before any release, as for npm
   ```
   A green **Actions → conformance** run on that commit counts instead of a local `make conformance`.
3. Bump `<Version>` in `dotnet/Directory.Build.props` in a PR and merge it to `main`. In the same PR, add the `## X.Y.Z` section to `dotnet/CHANGELOG.md` (the NuGet release notes link to it).
4. Tag the merged commit on `main` and push only that tag:
   ```sh
   git switch main && git pull --ff-only
   git tag dotnet-v0.2.0
   git push origin dotnet-v0.2.0
   ```
   Never `git push --tags`: every local `v*` or `dotnet-v*` tag would start a publish run.
5. Watch **Actions → publish-dotnet** and approve the `nuget` environment when verify is green.
6. Check the result (nuget.org indexes a new version within minutes; search can take longer):
   - nuget.org shows both packages at the version, with the README, the MIT licence, the repository link, and `Geekmotors.Crucible.Xunit` depending on `Geekmotors.Crucible (= X.Y.Z)`.
   - The package page's **Source Link** and **Deterministic** health indicators are green, and **Symbols** are listed (the `.snupkg` validation can take a little longer than the package).
   - In a scratch xUnit v3 test project: `dotnet add package Geekmotors.Crucible.Xunit --version X.Y.Z`, add a `crucible.config.json`, build, and check the file was copied to `bin/`; stepping into `Crucible` in a debugger loads the sources from GitHub.

### If a .NET release goes wrong

- **Tag/version mismatch, no CHANGELOG section, tag not on main, or a gate fails in verify.** Nothing was published. Delete the tag (`git push --delete origin dotnet-v0.2.0 && git tag -d dotnet-v0.2.0`), fix the cause on `main`, and tag again.
- **NuGet login fails (403 / no matching policy).** The trusted publishing policy does not match the repository, workflow file name or environment, or the policy's owner lost access. Fix the policy and re-run the failed publish job (the artifact is kept for 30 days).
- **One package pushed and the other failed.** Re-run only the failed publish job (**Re-run failed jobs**), within the artifact's 30 days; it pushes the same verified files and `--skip-duplicate` skips the one already there.
- **Once any package of a version is on nuget.org, never delete and re-tag that version.** A re-tagged run would skip the package already there (`--skip-duplicate`) and push the other one built from a different commit, and `Geekmotors.Crucible.Xunit` must match its exact engine build. Bump `<Version>` and release the next patch version instead.
- **You need to pull back a release.** NuGet versions are permanent and a version number can never be reused. Publish a fixed patch version and **unlist** the bad one on nuget.org (and optionally mark it deprecated); unlisting hides it from search but keeps existing restores working.
