namespace CrucibleAI.Tests.Constants;

internal static class OptionValues
{
    public const int Runs = 20;
    public const int NoRuns = 0;
    public const double Threshold = 0.95;
    public const long BeyondThirtyTwoBits = 2147483648L;
    public const string RunsKey = "runs";
    public const string RunsWithZeroFraction = """{ "runs": 20.0 }""";
    public const string ThresholdBeyondDoubleRange = """{ "threshold": 1e400 }""";
    public const string UnknownKeyHoldingNull = """{ "run": null }""";
}
