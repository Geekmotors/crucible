using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using CrucibleAI.Xunit.Tests.Constants;

namespace CrucibleAI.Xunit.Tests.Harnesses;

public class FixtureRunHarness : IAsyncLifetime
{
    private const string TypeField = "$type";
    private const string TestId = "TestUniqueID";
    private static readonly TimeSpan HangGuard = TimeSpan.FromMinutes(2);
    private readonly Dictionary<string, string> _names = new(StringComparer.Ordinal);
    private readonly List<FixtureResult> _results = [];

    protected virtual IEnumerable<string> Options => ["-maxThreads", "unlimited"];

    internal IReadOnlyList<FixtureResult> For(string test) =>
        _results.Where(result => result.DisplayName == test || result.DisplayName.StartsWith(test + "(", StringComparison.Ordinal)).ToArray();

    internal FixtureResult Single(string test) => Assert.Single(For(test));

    public async ValueTask InitializeAsync()
    {
        var folder = typeof(FixtureRunHarness).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == FixtureTests.FolderKey).Value!;
        var start = new ProcessStartInfo(DotnetHost())
        {
            WorkingDirectory = folder,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in new[] { "exec", Path.Combine(folder, FixtureTests.Assembly), "-reporter", "json", "-noColor" }.Concat(Options))
        {
            start.ArgumentList.Add(argument);
        }
        using var process = Process.Start(start)!;
        using var guard = new CancellationTokenSource(HangGuard);
        try
        {
            var errors = process.StandardError.ReadToEndAsync(guard.Token);
            while (await process.StandardOutput.ReadLineAsync(guard.Token) is { } line)
            {
                Read(line);
            }
            await errors;
            await process.WaitForExitAsync(guard.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private static string DotnetHost() =>
        Path.GetFullPath(Path.Combine(
            RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..", OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet"));

    private void Read(string line)
    {
        if (!line.StartsWith('{') || JsonNode.Parse(line) is not JsonObject message || message[TestId]?.GetValue<string>() is not { } id)
        {
            return;
        }
        var outcome = message[TypeField]?.GetValue<string>() switch
        {
            "test-starting" => Remember(id, message),
            "test-passed" => FixtureOutcome.Passed,
            "test-skipped" => FixtureOutcome.Skipped,
            "test-failed" => FixtureOutcome.Failed,
            _ => (FixtureOutcome?)null,
        };
        if (outcome is { } finished && _names.TryGetValue(id, out var name))
        {
            var types = message["ExceptionTypes"]?.AsArray().Select(type => type!.GetValue<string>()).ToArray() ?? [];
            _results.Add(new FixtureResult(name, finished, types, message["Messages"]?[0]?.GetValue<string>()));
        }
    }

    private FixtureOutcome? Remember(string id, JsonObject message)
    {
        _names[id] = message["TestDisplayName"]!.GetValue<string>();
        return null;
    }
}
