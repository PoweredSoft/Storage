using PoweredSoft.Storage.Core;

namespace PoweredSoft.Storage.Physical
{
    public class PhysicalDirectoryInfo : IDirectoryInfo
    {
        public PhysicalDirectoryInfo(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public bool IsDirectory => true;
    }
}
