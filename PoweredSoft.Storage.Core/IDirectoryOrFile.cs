namespace PoweredSoft.Storage.Core
{
    public interface IDirectoryOrFile
    {
        string Path { get; }
        bool IsDirectory { get; }
    }
}
