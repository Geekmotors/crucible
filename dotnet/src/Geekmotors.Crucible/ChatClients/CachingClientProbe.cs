using Microsoft.Extensions.AI;

namespace CrucibleAI.ChatClients;

internal sealed class CachingClientProbe
{
    private static readonly Lazy<Type?> CachingClientType =
        new(() => Type.GetType("Microsoft.Extensions.AI.CachingChatClient, Microsoft.Extensions.AI", false));

    public bool Caches(IChatClient client) => CachingClientType.Value is { } type && client.GetService(type) is not null;
}
