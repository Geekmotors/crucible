namespace CrucibleAI.Tests.Constants;

internal static class ChatClientTexts
{
    public const string Reply = """{"verdict":true,"reasoning":"consistent"}""";
    public const string ModelId = "judge-model-from-options";
    public const string ChangedModelId = "changed-by-the-client";
    public const string MetadataModelId = "judge-model-from-metadata";
    public const string BlankModelId = " ";
    public const string ClientDefaultModel = "(client default)";
    public const string ProviderName = "fake-provider";
    public const string ConversationId = "conversation-of-the-caller";
    public const string ClientFailure = "the chat client failed";
}
