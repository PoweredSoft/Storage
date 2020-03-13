using Microsoft.WindowsAzure.Storage.Blob;
using PoweredSoft.Storage.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.Storage.Azure.Blob
{
    public class AzureBlobFileInfo : IFileInfo
    {
        private readonly CloudBlockBlob fileBlock;

        public AzureBlobFileInfo(CloudBlockBlob fileBlock)
        {
            this.fileBlock = fileBlock;
        }

        public string FileName => System.IO.Path.GetFileName(fileBlock.Name);
        public string Extension => System.IO.Path.GetExtension(fileBlock.Name);
        public long FileSize => fileBlock.Properties.Length;
        public DateTimeOffset? CreatedTime => fileBlock.Properties.Created;
        public DateTimeOffset? LastModifiedTime => fileBlock.Properties.LastModified;
        public DateTimeOffset? LastAccessTime => null;
        public DateTime? CreatedTimeUtc => CreatedTime?.UtcDateTime;
        public DateTime? LastModifiedTimeUtc => LastModifiedTime?.UtcDateTime;
        public DateTime? LastAccessTimeUtc => null;
        public string Path => fileBlock.Uri.LocalPath.Replace($"/{fileBlock.Container.Name}/", "");
        public bool IsDirectory => false;
    }
}
