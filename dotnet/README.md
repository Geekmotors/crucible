# Crucible for .NET

**Coming soon.** `Geekmotors.Crucible` (the engine) and `Geekmotors.Crucible.Xunit` (the xUnit v3
adapter) are being ported from the TypeScript package. Their types live in the `CrucibleAI` namespace
(`using CrucibleAI;`), so `Crucible.Gate(...)` resolves from any namespace, `Geekmotors.*` included. Nothing is published to NuGet yet, and the
API is not ready for use.

The port follows [`spec/PORTING.md`](../spec/PORTING.md) and is proven against the shared fixtures
in [`spec/`](../spec/), the same answer key the TypeScript package runs.

## Writing Crucible tests with xUnit v3 (draft)

`Geekmotors.Crucible.Xunit` adds three attributes in the `CrucibleAI` namespace. Use `[CrucibleFact]`
instead of `[Fact]` and `[CrucibleTheory]` instead of `[Theory]`; the method reaches its run through
`Crucible.Facts`, `Crucible.CoherentAsync`, `Crucible.ExtractAsync<T>` and `Crucible.Context`.

```csharp
using CrucibleAI;

[Crucible(Runs = 10, Threshold = 0.9)]
public sealed class BookingAssistantTests(BookingFixture booking) : IClassFixture<BookingFixture>
{
    [CrucibleFact(Runs = 20)]
    public async Task ConfirmsTheBooking()
    {
        Crucible.Facts("The restaurant opens at 18:00.");
        var reply = await booking.Assistant.AskAsync("Book a table for two at seven", Crucible.Context.CancellationToken);
        Assert.True(await Crucible.CoherentAsync(reply, "The reply confirms a booking for two people"));
    }

    [CrucibleTheory(Runs = 5)]
    [InlineData("Book a table for four at eight", "a booking for four people")]
    [InlineData("Book a table for six at nine", "a booking for six people")]
    public async Task ConfirmsEachBooking(string request, string expectation)
    {
        var reply = await booking.Assistant.AskAsync(request, Crucible.Context.CancellationToken);
        Assert.True(await Crucible.CoherentAsync(reply, $"The reply confirms {expectation}"));
    }
}
```

Outside xUnit (NUnit, MSTest), call `Crucible.Gate(options, body, cancellationToken)` inside an ordinary test method
and pass the framework's test cancellation token.

- **Settings**: `Runs`, `Threshold`, `InferenceTimeout`, `TestTimeout`, `Strict` and `Sequential` on any of the
  three attributes. Nearest wins: the method attribute, then the class `[Crucible(...)]` (a nested class over its
  outer class, a derived class over its base), then `[assembly: Crucible(...)]`, then the config file's
  `testDefaults`, then the built-in defaults. A setting left out of an attribute is inherited (it reads `0` /
  `false` on the attribute object). An invalid value fails the test with a `usage` error.
- **One result per test**: each `[CrucibleFact]`, and each `[CrucibleTheory]` data row (`[InlineData]`,
  `[MemberData]`, `[ClassData]`), is one xUnit result: passed, or failed with `CrucibleVerdictException`, whose
  message is Crucible's failure report.
- **Runs**: all at once by default; `Sequential = true` runs them one after another. Each run gets its own
  test-class instance: the constructor (with xUnit's class, collection and assembly fixtures and
  `ITestOutputHelper`) and `IAsyncLifetime.InitializeAsync` run before the run starts, the method inside it, and
  `DisposeAsync`/`Dispose` after its body finishes. Constructors, `InitializeAsync`, `Dispose`, fixtures and static
  initialisers are outside every run: `Crucible.Facts` there is a `usage` error. A failing constructor, initialiser or
  dispose fails that run. After a halt or the deadline, a body still running is disposed when it finishes, which
  can be after the test has reported.
- **Isolation**: facts belong to one run of one test, never to the class, the collection, another row, another run
  or a test running in parallel. Crucible leaves xUnit's collections and parallelism to your settings.
- **Shared between runs**: the runs of one test share its class, collection and assembly fixtures, its theory row's
  argument objects and its `ITestOutputHelper` (lines from concurrent runs interleave; prefix them with
  `Crucible.Context.Run` when it matters). Keep fixtures thread-safe and row arguments immutable, or use
  `Sequential = true`.
- **Parallelism**: the runs execute on the thread pool, outside xUnit's `maxParallelThreads` limit: one test with
  `Runs = 10` makes up to 10 concurrent judge calls even under `-maxThreads 1` (which still works and does not
  deadlock).
- **Timeouts and cancellation**: `TestTimeout` is Crucible's deadline for all runs (it reports which runs did not
  finish). xUnit's own `Timeout` still applies to the whole test; leave it unset or set it above `TestTimeout`.
  xUnit's test cancellation (`TestContext.Current.CancellationToken`, cancelled by a timeout or an aborted run)
  ends the gate and cancels `Crucible.Context.CancellationToken`. With xUnit's `Timeout`, the result reports
  xUnit's `TestTimeoutException` alongside the gate's `OperationCanceledException`.
- **xUnit settings**: `Skip`, `SkipUnless`/`SkipWhen`, `Explicit`, `DisplayName` and traits keep their xUnit
  meaning; a skipped test runs no body and makes no judge calls. A run that calls `Assert.Skip` (or throws one of
  the attribute's `SkipExceptions`) skips the whole test. Test methods return `void`, `Task`, `Task<T>`,
  `ValueTask` or `ValueTask<T>`; `async void` and other awaitables are refused.

### The config file

Put `crucible.config.json` (or `crucible.config.jsonc`) beside the test project's `.csproj`. xUnit runs tests with
the build output folder as the working directory, so `Geekmotors.Crucible.Xunit` ships an MSBuild
`build/Geekmotors.Crucible.Xunit.targets` that copies the file there on every build (`PreserveNewest`); you do
not edit the project file. A missing config fails with a `config` error naming the folder searched.

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
| `src/Geekmotors.Crucible.Xunit/` | the xUnit v3 adapter (`[CrucibleFact]`, `[CrucibleTheory]`, `[Crucible]`) and its config-copy `build/` targets |
| `tests/Geekmotors.Crucible.Tests/` | xUnit v3 tests, including the `spec/` fixture runner (`Spec/`) |
| `tests/Geekmotors.Crucible.Xunit.Tests/` | adapter tests: settings resolution, and isolation proven by running the fixture project in a child process |
| `tests/Geekmotors.Crucible.Xunit.Fixture/` | real xUnit tests run only by the adapter tests (fake judge, never by `dotnet test`) |
| `tests/Geekmotors.Crucible.LiveTests/` | opt-in live smoke against OpenRouter (`make dotnet-live`) |

The fixture runner embeds every `spec/**/*.json` file at build time, validates each against
`spec/fixture.schema.json`, checks the case ids, and runs every case of the operations implemented
so far, one test per case id. Operations not ported yet are listed as pending in
`tests/Geekmotors.Crucible.Tests/Harnesses/SpecOperations.cs`; a fixture file whose operation is
neither implemented nor pending fails the run. Skipped cases, with their reasons, are listed there
too.
