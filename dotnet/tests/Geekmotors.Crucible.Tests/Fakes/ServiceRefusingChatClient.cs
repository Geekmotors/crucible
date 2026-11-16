using CrucibleAI.Tests.Constants;
using Microsoft.Extensions.AI;

namespace CrucibleAI.Tests.Fakes;

internal sealed class ServiceRefusingChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        throw new InvalidOperationException(ChatClientTexts.ClientFailure);

    public void Dispose()
    {
    }
}
