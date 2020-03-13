using PoweredSoft.Storage.Core;

namespace PoweredSoft.Storage.S3
{
    public class S3NotExistingDirectoryInfo : IDirectoryInfo
    {
        public S3NotExistingDirectoryInfo(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public bool IsDirectory => true;
    }
}