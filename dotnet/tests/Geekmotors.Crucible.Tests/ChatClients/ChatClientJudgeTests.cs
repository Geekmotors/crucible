using CrucibleAI.ChatClients;
using CrucibleAI.Providers;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Fakes;
using CrucibleAI.Tests.Harnesses;
using Microsoft.Extensions.AI;

namespace CrucibleAI.Tests.ChatClients;

public sealed class ChatClientJudgeTests
{
    private static readonly JudgeRequest Request = new(JudgeSettings.System, JudgeSettings.Prompt);

    [Fact]
    public async Task AnswersWithTheReplyText()
    {
        var reply = await CompleteAsync(new ChatClientJudge(new ScriptedChatClient(ChatClientTexts.Reply), null, new ImmediateBackoff()));

        Assert.Equal(ChatClientTexts.Reply, reply);
    }

    [Theory]
    [InlineData(ChatClientFailure.NetworkError)]
    [InlineData(ChatClientFailure.RequestTimeout)]
    [InlineData(ChatClientFailure.TooManyRequests)]
    [InlineData(ChatClientFailure.ServerError)]
    [InlineData(ChatClientFailure.ServiceUnavailable)]
    [InlineData(ChatClientFailure.Timeout)]
    [InlineData(ChatClientFailure.BrokenStream)]
    [InlineData(ChatClientFailure.ClientSideTimeout)]
    [InlineData(ChatClientFailure.SdkTooManyRequests)]
    [InlineData(ChatClientFailure.SdkBadGateway)]
    [InlineData(ChatClientFailure.AzureRequestTimeout)]
    [InlineData(ChatClientFailure.AzureServiceUnavailable)]
    [InlineData(ChatClientFailure.WrappedSdkTooManyRequests)]
    public async Task ATransientFailureIsRetried(ChatClientFailure failure)
    {
        var client = new ScriptedChatClient(ChatClientFailures.Create(failure), ChatClientTexts.Reply);

        var reply = await CompleteAsync(new ChatClientJudge(client, null, new ImmediateBackoff()));

        Assert.Equal(ChatClientTexts.Reply, reply);
    }

    [Fact]
    public async Task ATransientFailureOnEveryAttemptIsARetryableInfraError()
    {
        var failures = Enumerable.Range(0, TestLimits.ProviderAttempts).Select(_ => ChatClientFailures.Create(ChatClientFailure.ServiceUnavailable));
        var client = new ScriptedChatClient([.. failures, ChatClientTexts.Reply]);

        var error = await Assert.ThrowsAsync<CrucibleException>(() => CompleteAsync(new ChatClientJudge(client, null, new ImmediateBackoff())));

        Assert.Equal((CrucibleErrorKind.Infra, true), (error.Kind, error.Retryable));
    }

    [Theory]
    [InlineData(ChatClientFailure.BadRequest)]
    [InlineData(ChatClientFailure.Unauthorized)]
    [InlineData(ChatClientFailure.InvalidOperation)]
    [InlineData(ChatClientFailure.InvalidArgument)]
    [InlineData(ChatClientFailure.SdkBadRequest)]
    [InlineData(ChatClientFailure.AzureUnauthorized)]
    [InlineData(ChatClientFailure.WrappedSdkUnauthorized)]
    public async Task AnyOtherFailureIsAFatalInfraErrorWithoutARetry(ChatClientFailure failure)
    {
        var client = new ScriptedChatClient(ChatClientFailures.Create(failure), ChatClientTexts.Reply);

        var error = await Assert.ThrowsAsync<CrucibleException>(() => CompleteAsync(new ChatClientJudge(client, null, new ImmediateBackoff())));

        Assert.Equal((CrucibleErrorKind.Infra, false), (error.Kind, error.Retryable));
    }

    [Fact]
    public async Task CancellingTheCallCancelsTheClient()
    {
        using var cancellation = new CancellationTokenSource();
        var judge = new ChatClientJudge(new HangingChatClient(), null, new ImmediateBackoff());

        var call = judge.CompleteAsync(Request, new QuietJudgeObserver(), cancellation.Token);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
    }

    [Fact]
    public async Task EachCallGetsTheOptionsAsGiven()
    {
        var judge = new ChatClientJudge(new OptionsMutatingChatClient(), new ChatOptions { ModelId = ChatClientTexts.ModelId }, new ImmediateBackoff());

        await CompleteAsync(judge);
        var second = await CompleteAsync(judge);

        Assert.Equal(ChatClientTexts.ModelId, second);
    }

    [Fact]
    public async Task NoCallContinuesAServerSideConversation()
    {
        var judge = new ChatClientJudge(
            new ConversationEchoChatClient(), new ChatOptions { ConversationId = ChatClientTexts.ConversationId }, new ImmediateBackoff());

        var reply = await CompleteAsync(judge);

        Assert.Empty(reply);
    }

    private static Task<string> CompleteAsync(ChatClientJudge judge) =>
        judge.CompleteAsync(Request, new QuietJudgeObserver(), TestContext.Current.CancellationToken);
}
