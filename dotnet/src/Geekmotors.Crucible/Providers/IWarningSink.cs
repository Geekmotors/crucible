namespace Geekmotors.Crucible.Providers;

internal interface IWarningSink
{
    public void Write(string warning);
}
