using System;

namespace PoweredSoft.Storage.Core
{
    public interface IFileInfo : IDirectoryOrFile
    {
        string FileName { get; }
        string Extension { get; }
        long FileSize { get; }
        DateTimeOffset? CreatedTime { get; }
        DateTimeOffset? LastModifiedTime { get; }
        DateTimeOffset? LastAccessTime { get; }
        DateTime? CreatedTimeUtc { get; }
        DateTime? LastModifiedTimeUtc { get; }
        DateTime? LastAccessTimeUtc { get; }
    }
}
