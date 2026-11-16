using System.Text;

namespace CrucibleAI.Config;

internal sealed class PhysicalConfigFileSystem : IConfigFileSystem
{
    private static readonly UTF8Encoding Utf8KeepingByteOrderMark = new(false, false);

    public string ReadText(string path) => Utf8KeepingByteOrderMark.GetString(File.ReadAllBytes(path));

    public bool Exists(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path) || new FileInfo(path).LinkTarget is not null;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
