namespace CrucibleAI.Tests.Fakes;

internal sealed class RequestFailedShapedException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;

    public string? ErrorCode { get; init; }
}
