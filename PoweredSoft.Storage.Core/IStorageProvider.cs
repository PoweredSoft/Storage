using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.Storage.Core
{
    public interface IStorageProvider
    {
        Task<List<IDirectoryOrFile>> GetListAsync(string path);
        Task<List<IDirectoryInfo>> GetDirectories(string path);
        Task<List<IFileInfo>> GetFilesAsync(string path, string pattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        Task<IFileInfo> WriteFileAsync(string sourcePath, string path, bool overrideIfExists = true);
        Task<IFileInfo> WriteFileAsync(byte[] bytes, string path, bool overrideIfExists = true);
        Task<IFileInfo> WriteFileAsync(Stream stream, string path, bool overrideIfExists = true);
        Task<Stream> GetFileStreamAsync(string path);
        Task<byte[]> GetFileBytesAsync(string path);
        Task<string> GetFileContentAsync(string path, Encoding encoding);
        Task<bool> FileExistsAsync(string path);
        Task DeleteFileAsync(string path);
        Task DeleteDirectoryAsync(string path, bool force = false);
        Task<IDirectoryInfo> CreateDirectoryAsync(string path);

        bool IsFileNameAllowed(string fileName);
        string SanitizeFileName(string key, string replacement);
    }
}
