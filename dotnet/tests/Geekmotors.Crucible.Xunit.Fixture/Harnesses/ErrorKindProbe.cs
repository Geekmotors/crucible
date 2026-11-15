namespace CrucibleAI.Xunit.Fixture.Harnesses;

internal static class ErrorKindProbe
{
    public static CrucibleErrorKind? Of(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (CrucibleException failure)
        {
            return failure.Kind;
        }
    }
}
