using System;
using Amazon.S3.Model;
using PoweredSoft.Storage.Core;

namespace PoweredSoft.Storage.S3
{
    public class S3FileInfo : IFileInfo
    {
        public S3FileInfo(S3Object file)
        {
            Path = file.Key;
            CreatedTime = file.LastModified;
            LastModifiedTime = file.LastModified;
            LastAccessTime = file.LastModified;
            CreatedTimeUtc = file.LastModified.ToUniversalTime();
            LastAccessTimeUtc = file.LastModified.ToUniversalTime(); 
            LastModifiedTimeUtc = file.LastModified.ToUniversalTime();
            FileSize = file.Size;
        }

        public string FileName => System.IO.Path.GetFileName(Path);
        public string Extension => System.IO.Path.GetExtension(Path);
        public long FileSize { get; }
        public DateTimeOffset? CreatedTime { get; }
        public DateTimeOffset? LastModifiedTime { get; }
        public DateTimeOffset? LastAccessTime { get; }
        public DateTime? CreatedTimeUtc { get; }
        public DateTime? LastModifiedTimeUtc { get; }
        public DateTime? LastAccessTimeUtc { get; }
        public string Path { get; }
        public bool IsDirectory => false;
    }
}