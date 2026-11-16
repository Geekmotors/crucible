namespace CrucibleAI.Text;

internal static class EcmaScriptMath
{
    private const double Half = 0.5;

    public static double Round(double value)
    {
        var floor = Math.Floor(value);
        return value - floor >= Half ? floor + 1 : floor;
    }
}
