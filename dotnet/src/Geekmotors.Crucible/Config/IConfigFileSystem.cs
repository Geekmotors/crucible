namespace CrucibleAI.Config;

internal interface IConfigFileSystem
{
    public string ReadText(string path);

    public bool Exists(string path);
}
