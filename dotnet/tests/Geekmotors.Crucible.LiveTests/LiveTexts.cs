namespace CrucibleAI.LiveTests;

internal static class LiveTexts
{
    public const string KeyVariable = "CRUCIBLE_JUDGE_KEY";
    public const string NoKey = "Set CRUCIBLE_JUDGE_KEY (an OpenRouter key) to run the live smoke; make dotnet-live does.";
    public const string Response = "The package was delivered to the front desk at 09:15 and signed for by the receptionist.";
    public const string Claim = "The reply says the package was delivered.";
    public const string Target = "the hour of the delivery time, from 0 to 23";
    public const int DeliveryHour = 9;
    public const int SmokeRuns = 1;
    public const double EveryRun = 1;
    public const string ArrivalResponse = "The train from Leeds arrived at platform 4 twelve minutes late.";
    public const string ArrivalClaim = "The reply says the train was late.";
}
