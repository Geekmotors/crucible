using CrucibleAI.Tests.Constants;
using Microsoft.Extensions.AI;

namespace CrucibleAI.Tests.Fakes;

internal sealed class DescribedChatClient : IChatClient
{
    private readonly ChatClientMetadata _metadata = new(ChatClientTexts.ProviderName, null, ChatClientTexts.MetadataModelId);

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, ChatClientTexts.Reply)));

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType.IsInstanceOfType(_metadata) ? _metadata : serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose()
    {
    }
}
