using CrucibleAI.Judging;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Judging;

public sealed class JudgeExchangeTests
{
    private readonly ManualTimeProvider _clock = new();

    [Fact]
    public async Task AJudgeCallPastTheInferenceTimeoutIsARetryableInfraErrorAndIsNotAskedAgain()
    {
        var call = Exchange().AskAsync(ProviderHarness.Request, new CoherentReplyReader().Read, JudgeTimes.InferenceTimeout, new QuietJudgeObserver(), CancellationToken.None);

        _clock.Advance(JudgeTimes.InferenceTimeout);

        var error = await Assert.ThrowsAsync<CrucibleException>(() => call);
        Assert.Equal((CrucibleErrorKind.Infra, true), (error.Kind, error.Retryable));
    }

    [Fact]
    public async Task ACancelledRunIsNotATimeout()
    {
        using var cancellation = new CancellationTokenSource();
        var call = Exchange().AskAsync(ProviderHarness.Request, new CoherentReplyReader().Read, JudgeTimes.InferenceTimeout, new QuietJudgeObserver(), cancellation.Token);

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
    }

    private JudgeExchange Exchange() =>
        new(new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(HttpAnswers.Never()), _clock);
}
