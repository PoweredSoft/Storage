using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PoweredSoft.Storage.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.Storage.Azure.Blob
{
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private string connectionString = null;
        private string containerName = null;

        public AzureBlobStorageProvider() 
        {
            
        }

        public AzureBlobStorageProvider(string connectionString, string containerName)
        {
            this.SetConnectionString(connectionString);
            this.SetContainerName(containerName);
        }   

        public void SetContainerName(string name)
        {
            this.containerName = name;
        }

        public void SetConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Task<IDirectoryInfo> CreateDirectoryAsync(string path)
        {
            var ret = new AzureBlobNotExistingDirectoryInfo(path);
            return Task.FromResult<IDirectoryInfo>(ret);
        }

        public async Task DeleteDirectoryAsync(string path, bool force = false)
        {
            var ret = new List<IDirectoryOrFile>();
            var container = GetContainer();
            var finalPath = CleanDirectoryPath(path);

            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                BlobResultSegment response;
                if (continuationToken == null)
                    response = await container.ListBlobsSegmentedAsync(finalPath, true, BlobListingDetails.All, null, continuationToken, null, null);
                else
                    response = await container.ListBlobsSegmentedAsync(continuationToken);

                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            var files = results.Where(t => t is CloudBlockBlob).Cast<CloudBlockBlob>().ToList();
            foreach (var file in files)
                await this.DeleteFileAsync(file.Name);
        }

        public Task DeleteFileAsync(string path)
        {
            return GetContainer().GetBlobReference(path).DeleteIfExistsAsync();
        }

        public Task<bool> FileExistsAsync(string path)
        {
            return GetContainer().GetBlobReference(path).ExistsAsync();
        }

        public async Task<List<IDirectoryInfo>> GetDirectories(string path)
        {
            return (await this.GetListAsync(path)).Where(t => t.IsDirectory).Cast<IDirectoryInfo>().ToList();
        }

        public async Task<List<IFileInfo>> GetFilesAsync(string path, string pattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (pattern != null)
                throw new NotSupportedException("Blob Storage does not support glob searching only prefix.");

            var result = await GetListAsync(path);
            var finalResult = result.Where(t => !t.IsDirectory).Cast<IFileInfo>().ToList();
            return finalResult;
        }

        private string CleanDirectoryPath(string path)
        {
            if (path == null)
                return path;

            path = path.TrimEnd('/');

            if (path != "")
                path += "/";

            return path;
        }

        private CloudBlobContainer GetContainer()
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            return container;
        }

        public async Task<List<IDirectoryOrFile>> GetListAsync(string path)
        {
            var ret = new List<IDirectoryOrFile>();
            var container = GetContainer();
            var finalPath = CleanDirectoryPath(path);

            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                BlobResultSegment response;
                if (continuationToken == null)
                    response = await container.ListBlobsSegmentedAsync(finalPath, continuationToken);
                else
                    response = await container.ListBlobsSegmentedAsync(continuationToken);

                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            foreach (var result in results)
            {
                if (result is CloudBlobDirectory blobDirectory)
                    ret.Add(new AzureBlobDirectoryInfo(blobDirectory));
                else if (result is CloudBlockBlob blobBlock)
                    ret.Add(new AzureBlobFileInfo(blobBlock));
            }

            return ret;
        }

        public Task<IFileInfo> WriteFileAsync(string sourcePath, string path, bool overrideIfExists = true)
        {
            return WriteFileAsync(sourcePath, path, new DefaultWriteOptions
            {
                OverrideIfExists = overrideIfExists
            });
        }

        public Task<IFileInfo> WriteFileAsync(byte[] bytes, string path, bool overrideIfExists = true)
        {
            return WriteFileAsync(bytes, path, new DefaultWriteOptions
            {
                OverrideIfExists = overrideIfExists
            });
        }

        public Task<IFileInfo> WriteFileAsync(Stream stream, string path, bool overrideIfExists = true)
        {
            return WriteFileAsync(stream, path, new DefaultWriteOptions
            {
                OverrideIfExists = overrideIfExists
            });
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            await ThrowNotExistingAsync(path);
            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);
            return await blob.OpenReadAsync();
        }

        private async Task ThrowNotExistingAsync(string path)
        {
            if (false == await this.FileExistsAsync(path))
                throw new FileDoesNotExistException(path);
        }

        public async Task<byte[]> GetFileBytesAsync(string path)
        {
            await ThrowNotExistingAsync(path);
            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);
            var bytes = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(bytes, 0);
            return bytes;
        }

        public async Task<string> GetFileContentAsync(string path, Encoding encoding)
        {
            await ThrowNotExistingAsync(path);
            var container = GetContainer();
            return encoding.GetString(await this.GetFileBytesAsync(path));
        }

        public bool IsFileNameAllowed(string fileName)
        {
            return true;
        }

        public string SanitizeFileName(string key, string replacement)
        {
            return key;
        }

        public async Task<IFileInfo> WriteFileAsync(string sourcePath, string path, IWriteFileOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.OverrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);
            await blob.UploadFromFileAsync(sourcePath);
            return new AzureBlobFileInfo(blob);
        }

        public async Task<IFileInfo> WriteFileAsync(byte[] bytes, string path, IWriteFileOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.OverrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
            return new AzureBlobFileInfo(blob);
        }

        public async Task<IFileInfo> WriteFileAsync(Stream stream, string path, IWriteFileOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.OverrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);
         
            if (stream.CanSeek && stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);
            await blob.UploadFromStreamAsync(stream);
            return new AzureBlobFileInfo(blob);
        }
    }
}
