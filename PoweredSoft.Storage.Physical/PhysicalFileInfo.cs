using PoweredSoft.Storage.Core;
using System;
using System.IO;

namespace PoweredSoft.Storage.Physical
{
    public class PhysicalFileInfo : IFileInfo
    {
        private readonly FileInfo fileInfo;

        public PhysicalFileInfo(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public string Path => fileInfo.FullName;
        public string FileName => fileInfo.Name;
        public string Extension => fileInfo.Extension;
        public long FileSize => fileInfo.Length;
        public DateTimeOffset? CreatedTime => fileInfo.CreationTime;
        public DateTimeOffset? LastModifiedTime => fileInfo.LastWriteTime;
        public DateTimeOffset? LastAccessTime => fileInfo.LastAccessTime;
        public DateTime? CreatedTimeUtc => fileInfo.CreationTimeUtc;
        public DateTime? LastModifiedTimeUtc => fileInfo.LastWriteTimeUtc;
        public DateTime? LastAccessTimeUtc => fileInfo.LastAccessTimeUtc;
        public bool IsDirectory => false;
    }
}
