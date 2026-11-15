using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Providers;

public sealed class ChatCompletionsClientTests
{
    [Fact]
    public async Task WarnsOnceWhenAKeyTravelsOverPlainHttpToARemoteHost()
    {
        var harness = new ProviderHarness(JudgeSettings.PlainHttpConfig);
        var client = harness.Client(HttpAnswers.Reply(JudgeSettings.Reply), HttpAnswers.Reply(JudgeSettings.Reply));

        await client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None);
        await client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None);

        Assert.Single(harness.Warnings.Lines);
    }

    [Fact]
    public async Task ThePlainHttpWarningNeverHoldsTheKey()
    {
        var harness = new ProviderHarness(JudgeSettings.PlainHttpConfig);

        await harness.Client(HttpAnswers.Reply(JudgeSettings.Reply)).CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None);

        Assert.DoesNotContain(JudgeSettings.Key, string.Concat(harness.Warnings.Lines), StringComparison.Ordinal);
    }

    [Fact]
    public async Task NoWarningOverHttps()
    {
        var harness = new ProviderHarness(JudgeSettings.HttpsCompatibleConfig);

        await harness.Client(HttpAnswers.Reply(JudgeSettings.Reply)).CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None);

        Assert.Empty(harness.Warnings.Lines);
    }

    [Fact]
    public async Task SendsToThePathExactlyAsTheBaseUrlSerialises()
    {
        var client = new ProviderHarness(JudgeSettings.EscapedPathConfig)
            .Client(HttpAnswers.ServedOnlyAt(JudgeSettings.EscapedPathEndpoint, JudgeSettings.Reply));

        Assert.Equal(JudgeSettings.Reply, await client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None));
    }

    [Fact]
    public async Task ACancelledCallEndsWithoutRetrying()
    {
        using var cancellation = new CancellationTokenSource();
        var call = new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(HttpAnswers.Never())
            .CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), cancellation.Token);

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
    }

    [Fact]
    public async Task AnInputOutputFailureIsRetried()
    {
        var client = new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(HttpAnswers.BrokenStream(), HttpAnswers.Reply(JudgeSettings.Reply));

        Assert.Equal(JudgeSettings.Reply, await client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None));
    }

    [Fact]
    public async Task AFailureAfterCancellationEndsAsACancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var client = new ProviderHarness(JudgeSettings.OpenRouterConfig).Client(HttpAnswers.ServerErrorWhileCancelling(cancellation));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), cancellation.Token));
    }

    [Fact]
    public async Task ReadsTheReplyAsUtf8WhateverCharsetTheResponseDeclares()
    {
        var client = new ProviderHarness(JudgeSettings.OpenRouterConfig)
            .Client(HttpAnswers.ReplyDeclaringCharset(JudgeSettings.Reply, JudgeSettings.UnknownCharset));

        Assert.Equal(JudgeSettings.Reply, await client.CompleteAsync(ProviderHarness.Request, new QuietJudgeObserver(), CancellationToken.None));
    }

    [Fact]
    public void ABaseUrlHostTheHttpStackCannotAddressIsAConfigError()
    {
        var harness = new ProviderHarness(JudgeSettings.HostUnaddressableByUriConfig);

        var error = Assert.Throws<CrucibleException>(() => harness.Client());

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }
}
