namespace CrucibleAI.Tests.Constants;

internal static class AssertionTexts
{
    public const string Response = "The meeting is at six.";
    public const string Claim = "The reply names a time";
    public const string Target = "the meeting time";
    public const string RunFact = "Run fact.";
    public const string CallFact = "Call fact.";
    public const string LateFact = "Late fact.";
    public const double ExtractedNumber = 6;
    public const double Fraction = 2.5;
    public const double BeyondInt32 = 3e9;
    public const int WholeNumber = 6;
    public const long LongNumber = 3_000_000_000;
    public const int ProviderAttempts = 3;
    public const int UnusableReplyAsks = 2;
    public const int ServerError = 500;
    public const string UnusableReply = "not json";
    public const string CoherentSchema = "coherent.md";
    public const string ExtractSchema = "extract.md";
}
