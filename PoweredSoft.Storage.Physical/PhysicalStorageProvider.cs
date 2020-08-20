using PoweredSoft.Storage.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.Storage.Physical
{
    public class PhysicalStorageProvider : IStorageProvider
    {
        public Task<IDirectoryInfo> CreateDirectoryAsync(string path)
        {
            var directoryInfo = System.IO.Directory.CreateDirectory(path);
            var result = new PhysicalDirectoryInfo(path);
            return Task.FromResult<IDirectoryInfo>(result);
        }

        public Task DeleteDirectoryAsync(string path, bool force = false)
        {
            if (force)
                Directory.Delete(path, true);
            else
                Directory.Delete(path);

            return Task.CompletedTask;
        }

        public Task DeleteFileAsync(string path)
        {
            System.IO.File.Delete(path);
            return Task.CompletedTask;
        }

        public Task<bool> FileExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(path));
        }

        public Task<List<IDirectoryInfo>> GetDirectories(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var directories = directoryInfo.GetDirectories();
            var directoriesConverted = directories.Select(t => new PhysicalDirectoryInfo(t.FullName)).AsEnumerable<IDirectoryInfo>().ToList();
            return Task.FromResult(directoriesConverted);
        }

        public async Task<byte[]> GetFileBytesAsync(string path)
        {
            await ThrowNotExistingAsync(path);
            return File.ReadAllBytes(path);
        }

        public async Task<string> GetFileContentAsync(string path, Encoding encoding)
        {
            await ThrowNotExistingAsync(path);
            return File.ReadAllText(path, encoding);
        }

        public Task<List<IFileInfo>> GetFilesAsync(string path, string pattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var directoryInfo = new DirectoryInfo(path);

            FileInfo[] files;
            if (string.IsNullOrWhiteSpace(pattern))
                files = directoryInfo.GetFiles();
            else
                files = directoryInfo.GetFiles(pattern, searchOption);

            var result = files.Select(fileInfo => new PhysicalFileInfo(fileInfo)).AsEnumerable<IFileInfo>().ToList();
            return Task.FromResult(result);
        }

        private async Task ThrowNotExistingAsync(string path)
        {
            if (false == await this.FileExistsAsync(path))
                throw new FileDoesNotExistException(path);
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            await ThrowNotExistingAsync(path);
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public async Task<List<IDirectoryOrFile>> GetListAsync(string path)
        {
            var files = await this.GetFilesAsync(path);
            var directories = await this.GetDirectories(path);
            var result = files.AsEnumerable<IDirectoryOrFile>().Concat(directories.AsEnumerable<IDirectoryOrFile>()).ToList();
            return result;
        }

        public async Task<IFileInfo> WriteFileAsync(string sourcePath, string path, bool overrideIfExists = true)
        {
            if (!overrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            CreateDirectoryIfNotExisting(path);

            System.IO.File.Copy(sourcePath, path, overrideIfExists);
            var fileInfo = new FileInfo(path);
            var ret = new PhysicalFileInfo(fileInfo);
            return ret;
        }

        public async Task<IFileInfo> WriteFileAsync(byte[] bytes, string path, bool overrideIfExists = true)
        {
            if (!overrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            CreateDirectoryIfNotExisting(path);

            File.WriteAllBytes(path, bytes);
            var fileInfo = new FileInfo(path);
            var physicalinfo = new PhysicalFileInfo(fileInfo);
            return physicalinfo;
        }

        public async Task<IFileInfo> WriteFileAsync(Stream stream, string path, bool overrideIfExists = true)
        {
            if (!overrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            CreateDirectoryIfNotExisting(path);

            if (stream.CanSeek && stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
                fileStream.Close();
            }

            var fileInfo = new FileInfo(path);
            var physicalinfo = new PhysicalFileInfo(fileInfo);
            return physicalinfo;
        }

        private void CreateDirectoryIfNotExisting(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public bool IsFileNameAllowed(string fileName)
        {
            return true;
        }

        public string SanitizeFileName(string key, string replacement)
        {
            return key;
        }
    }
}
