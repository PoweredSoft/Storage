using Microsoft.WindowsAzure.Storage.Blob;
using PoweredSoft.Storage.Core;

namespace PoweredSoft.Storage.Azure.Blob
{
    public class AzureBlobDirectoryInfo : IDirectoryInfo
    {
        private CloudBlobDirectory blobDirectory;

        public AzureBlobDirectoryInfo(CloudBlobDirectory blobDirectory)
        {
            this.blobDirectory = blobDirectory;
        }

        public string Path => blobDirectory.Prefix.TrimEnd('/');
        public bool IsDirectory => true;
    }

    public class AzureBlobNotExistingDirectoryInfo : IDirectoryInfo
    {
        public AzureBlobNotExistingDirectoryInfo(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public bool IsDirectory => true;
    }
}