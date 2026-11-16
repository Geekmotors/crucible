using CrucibleAI.Tests.Constants;
using Microsoft.Extensions.AI;

namespace CrucibleAI.Tests.Fakes;

internal sealed class OptionsMutatingChatClient : IChatClient
{
    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var seen = options?.ModelId ?? string.Empty;
        if (options is not null)
        {
            options.ModelId = ChatClientTexts.ChangedModelId;
        }
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, seen)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose()
    {
    }
}
