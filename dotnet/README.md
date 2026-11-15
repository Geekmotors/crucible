# Crucible for .NET

**Coming soon.** `Geekmotors.Crucible` (the engine) and `Geekmotors.Crucible.Xunit` (the xUnit v3
adapter) are being ported from the TypeScript package. Nothing is published to NuGet yet, and the
API is not ready for use.

The port follows [`spec/PORTING.md`](../spec/PORTING.md) and is proven against the shared fixtures
in [`spec/`](../spec/), the same answer key the TypeScript package runs.

## Build and test

Needs the .NET 10 SDK (see `global.json`) and the .NET 8 runtime, because the library and its
tests target `net8.0` and `net10.0`.

```sh
cd dotnet
dotnet build -warnaserror
dotnet test
dotnet format --verify-no-changes
```

Or from the repository root: `make dotnet-test` and `make dotnet-format`.

## Layout

| Path | What |
| --- | --- |
| `src/Geekmotors.Crucible/` | the engine library |
| `tests/Geekmotors.Crucible.Tests/` | xUnit v3 tests, including the `spec/` fixture runner (`Spec/`) |

The fixture runner embeds every `spec/**/*.json` file at build time, validates each against
`spec/fixture.schema.json`, checks the case ids, and runs every case of the operations implemented
so far, one test per case id. Operations not ported yet are listed as pending in
`tests/Geekmotors.Crucible.Tests/Harnesses/SpecOperations.cs`; a fixture file whose operation is
neither implemented nor pending fails the run. Skipped cases, with their reasons, are listed there
too.
