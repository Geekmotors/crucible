namespace CrucibleAI.Tests.Harnesses;

internal sealed class InexpressibleInputException(string input) : Exception($"C# cannot express the fixture input {input}; skip the case by id.");
