namespace CrucibleAI.Tests.Fakes;

internal sealed class ClientResultShapedException(string message, int status) : Exception(message)
{
    public int Status { get; } = status;
}
