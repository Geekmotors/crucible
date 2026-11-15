namespace CrucibleAI.Tests.Harnesses;

internal sealed class UnknownSpecialTokenException(string token) : Exception(token);
