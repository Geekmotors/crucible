namespace Geekmotors.Crucible.Providers;

internal sealed class StandardErrorWarningSink : IWarningSink
{
    public void Write(string warning)
    {
        try
        {
            Console.Error.WriteLine(warning);
        }
        catch (IOException)
        {
        }
    }
}
