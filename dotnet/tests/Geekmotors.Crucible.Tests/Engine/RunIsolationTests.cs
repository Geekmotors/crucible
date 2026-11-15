using CrucibleAI.Engine;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Engine;

public sealed class RunIsolationTests
{
    private readonly EngineHarness _harness = new();
    private readonly IsolationProbe _probe = new();

    [Fact(Timeout = TestLimits.HangGuardMs)]
    public async Task ConcurrentRunsOfOneGateSeeOnlyTheirOwnFacts()
    {
        var together = new Rendezvous(EngineRuns.Many, TestContext.Current.CancellationToken);

        await _harness.RunAsync(EngineRuns.AllOfMany, async context =>
        {
            Crucible.Facts(EngineHarness.Fact(context.Run));
            await together.ArriveAsync();

            var prompt = await EngineHarness.PromptAsync();

            Assert.Equal(
                [context.Run],
                Enumerable.Range(1, EngineRuns.Many).Where(run => prompt.Contains(EngineHarness.Fact(run), StringComparison.Ordinal)));
        });
    }

    [Fact]
    public async Task FactsAddedAfterAnAwaitStayInTheRun()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            await Task.Yield();
            Crucible.Facts(EngineHarness.Fact(context.Run));

            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task FactsAddedInTaskRunStayInTheRun()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            await Task.Run(() => Crucible.Facts(EngineHarness.Fact(context.Run)));

            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task FactsAddedInAContinuationStayInTheRun()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var continuation = started.Task.ContinueWith(_ => Crucible.Facts(EngineHarness.Fact(context.Run)), TaskScheduler.Default);
            started.SetResult();
            await continuation;

            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task FactsAddedInsideAnAwaitedHelperReachTheRestOfTheRun()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            await _probe.AddFactLaterAsync(EngineHarness.Fact(context.Run));

            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task TheContextReachedInsideAnAwaitedHelperIsTheRunsOwn()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            var reached = await _probe.ContextLaterAsync();

            Assert.Same(context, reached);
        });
    }

    [Fact]
    public async Task ACallOutsideAnyRunIsAUsageError()
    {
        var errors = new[]
        {
            Record.Exception(() => Crucible.Facts(EngineTexts.LateFact)),
            Record.Exception(() => Crucible.Context),
            await Record.ExceptionAsync(() => Crucible.CoherentAsync(EngineTexts.Response, EngineTexts.Claim, cancellationToken: TestContext.Current.CancellationToken)),
            await Record.ExceptionAsync(() => Crucible.ExtractAsync<string>(EngineTexts.Response, EngineTexts.Target, cancellationToken: TestContext.Current.CancellationToken)),
        };

        Assert.All(errors, error => Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(error).Kind));
    }

    [Fact]
    public async Task ACallThroughTheContextAfterItsRunSettledIsAUsageError()
    {
        CrucibleContext? captured = null;
        await _harness.RunAsync(EngineRuns.Smoke, context =>
        {
            captured = context;
            return Task.CompletedTask;
        });

        var errors = new[]
        {
            Record.Exception(() => captured!.Facts(EngineTexts.LateFact)),
            await Record.ExceptionAsync(() => captured!.CoherentAsync(EngineTexts.Response, EngineTexts.Claim, cancellationToken: TestContext.Current.CancellationToken)),
            await Record.ExceptionAsync(() => captured!.ExtractAsync<string>(EngineTexts.Response, EngineTexts.Target, cancellationToken: TestContext.Current.CancellationToken)),
        };

        Assert.All(errors, error => Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(error).Kind));
    }

    [Fact(Timeout = TestLimits.HangGuardMs)]
    public async Task ABackgroundTaskOutlivingItsRunCannotWriteIntoALaterRun()
    {
        var later = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var background = new ThrownCapture();
        Task? outliving = null;
        await _harness.RunAsync(EngineRuns.Smoke, _ =>
        {
            outliving = later.Task.ContinueWith(_ => background.Run(() => Crucible.Facts(EngineTexts.LateFact)), TaskScheduler.Default);
            return Task.CompletedTask;
        });

        string? laterPrompt = null;
        await _harness.RunAsync(EngineRuns.Smoke, async _ =>
        {
            later.SetResult();
            await outliving!.WaitAsync(TestContext.Current.CancellationToken);
            laterPrompt = await EngineHarness.PromptAsync();
        });

        Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(await background.Thrown).Kind);
        Assert.DoesNotContain(EngineTexts.LateFact, laterPrompt, StringComparison.Ordinal);
    }

    [Fact(Timeout = TestLimits.HangGuardMs)]
    public async Task TwoGatesRunningConcurrentlyNeverSeeEachOthersFacts()
    {
        var together = new Rendezvous(EngineRuns.Few * 2, TestContext.Current.CancellationToken);

        await Task.WhenAll(
            _harness.RunAsync(EngineRuns.AllOfFew, context => _probe.SeesOnlyItsOwnAsync(EngineTexts.FirstGate, EngineTexts.SecondGate, context, together)),
            _harness.RunAsync(EngineRuns.AllOfFew, context => _probe.SeesOnlyItsOwnAsync(EngineTexts.SecondGate, EngineTexts.FirstGate, context, together)));
    }

    [Fact]
    public async Task SuppressedFlowLosesTheAmbientRunButNotTheContext()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            var ambient = new ThrownCapture();
            var explicitly = new ThrownCapture();
            Task suppressed;
            using (ExecutionContext.SuppressFlow())
            {
                suppressed = Task.Run(() =>
                {
                    ambient.Run(() => Crucible.Facts(EngineTexts.LateFact));
                    explicitly.Run(() => context.Facts(EngineHarness.Fact(context.Run)));
                });
            }
            await suppressed;

            Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(await ambient.Thrown).Kind);
            Assert.Null(await explicitly.Thrown);
            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task AnUnsafeThreadPoolCallbackLosesTheAmbientRunButNotTheContext()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            var ambient = new ThrownCapture();
            var explicitly = new ThrownCapture();
            ThreadPool.UnsafeQueueUserWorkItem(
                _ =>
                {
                    ambient.Run(() => Crucible.Facts(EngineTexts.LateFact));
                    explicitly.Run(() => context.Facts(EngineHarness.Fact(context.Run)));
                },
                null);

            Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(await ambient.Thrown).Kind);
            Assert.Null(await explicitly.Thrown);
            Assert.Contains(EngineHarness.Fact(context.Run), await EngineHarness.PromptAsync(), StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task TheBodyFactoryRunsOutsideTheRun()
    {
        var error = await Assert.ThrowsAsync<CrucibleException>(() => _harness.RunAsync(new GateRequest(
            [],
            EngineRuns.Smoke,
            _ =>
            {
                Crucible.Facts(EngineTexts.LateFact);
                return Task.FromResult(RunBody.Of(_ => Task.CompletedTask));
            },
            false)));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public async Task TheCallerHasNoRunAfterTheGateReturns()
    {
        await _harness.RunAsync(EngineRuns.AllOfFew, _ => Task.CompletedTask);

        var error = Record.Exception(() => Crucible.Context);

        Assert.Equal(CrucibleErrorKind.Usage, Assert.IsType<CrucibleException>(error).Kind);
    }

    [Fact]
    public async Task ANestedGateRunsInItsOwnRunAndTheOuterRunResumesAfterIt()
    {
        await _harness.RunAsync(EngineRuns.Smoke, async outer =>
        {
            CrucibleContext? inner = null;
            await _harness.RunAsync(EngineRuns.Smoke, context =>
            {
                inner = Crucible.Context;
                Assert.Same(context, inner);
                return Task.CompletedTask;
            });

            Assert.NotSame(outer, inner);
            Assert.Same(outer, Crucible.Context);
        });
    }

    [Fact(Timeout = TestLimits.HangGuardMs)]
    public async Task AGateBlockedOnFromASingleThreadedSynchronizationContextStillCompletes()
    {
        var caller = new BlockingCaller(new StalledSynchronizationContext());

        var error = await caller.BlockOnAsync(() => _harness.RunAsync(EngineRuns.AllOfFew, async context =>
        {
            await Task.Yield();
            Crucible.Facts(EngineHarness.Fact(context.Run));
            await EngineHarness.PromptAsync();
        })).WaitAsync(TestContext.Current.CancellationToken);

        Assert.Null(error);
    }
}
