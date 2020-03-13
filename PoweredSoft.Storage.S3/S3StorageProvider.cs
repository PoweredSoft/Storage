using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using PoweredSoft.Storage.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.Storage.S3
{
    public class S3StorageProvider : IStorageProvider
    {
        protected readonly string endpoint;
        protected readonly string bucketName;
        protected readonly string accessKey;
        protected readonly string secret;

        protected S3UsEast1RegionalEndpointValue? s3UsEast1RegionalEndpointValue;
        protected bool forcePathStyle = false;

        public S3StorageProvider(string endpoint, string bucketName, string accessKey, string secret)
        {
            this.endpoint = endpoint;
            this.bucketName = bucketName;
            this.accessKey = accessKey;
            this.secret = secret;
        }

        public void SetForcePathStyle(bool forcePathStyle)
        {
            this.forcePathStyle = forcePathStyle;
        }

        public void SetS3UsEast1RegionalEndpointValue(S3UsEast1RegionalEndpointValue value)
        {
            this.s3UsEast1RegionalEndpointValue = value;
        }

        protected virtual IAmazonS3 GetClient()
        {
            var config = new AmazonS3Config
            {
                USEast1RegionalEndpointValue = s3UsEast1RegionalEndpointValue,
                ServiceURL = endpoint,
                ForcePathStyle = forcePathStyle
            };
            var client = new AmazonS3Client(this.accessKey, this.secret, config);
            return client;
        }
     
        public Task<IDirectoryInfo> CreateDirectoryAsync(string path)
        {
            return Task.FromResult<IDirectoryInfo>(new S3NotExistingDirectoryInfo(path));
        }

        /// <summary>
        /// Can only delete 1000 at a time.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public async Task DeleteDirectoryAsync(string path, bool force = false)
        {
            using var client = GetClient();
            var files = await this.GetS3FilesAsync(prefix: path, delimiter: null);
            var next = files.AsQueryable();
            while(next.Any())
            {
                var next1000 = next.Take(1000);
                var keys = next1000.Select(s3o => new KeyVersion { Key = s3o.Key }).ToList();
                await client.DeleteObjectsAsync(new DeleteObjectsRequest
                {
                    BucketName = this.bucketName,
                    Objects = keys
                });

                next = next.Skip(1000);
            }
        }

        public async Task DeleteFileAsync(string path)
        {
            using var client = GetClient();
            var response = await client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = this.bucketName,
                Key = path
            });
        }

        public async Task<bool> FileExistsAsync(string path)
        {
            var item = await GetS3FileByPath(path);
            return item != null;
        }

        public Task<List<IDirectoryInfo>> GetDirectories(string path)
        {
            return Task.FromResult(new List<IDirectoryInfo>());
        }

        public async Task<byte[]> GetFileBytesAsync(string path)
        {
            using var fileStream = await this.GetFileStreamAsync(path);
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<string> GetFileContentAsync(string path, Encoding encoding)
        {
            using var fileStream = await this.GetFileStreamAsync(path);
            using var streamReader = new StreamReader(fileStream, encoding);
            return await streamReader.ReadToEndAsync();
        }

        public async Task<List<IFileInfo>> GetFilesAsync(string path, string pattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (pattern != null)
                throw new NotSupportedException();

            string finalPath = SantizeDirectoryRequest(path);
            var s3Files = await this.GetS3FilesAsync(prefix: finalPath, delimiter: "/");
            var ret = s3Files.Select(s3 => new S3FileInfo(s3)).AsEnumerable<IFileInfo>().ToList();
            return ret;
        }

        private static string SantizeDirectoryRequest(string path)
        {
            string finalPath;
            if (path == "/")
                finalPath = "";
            else
                finalPath = $"{path?.TrimEnd('/')}/";
            return finalPath;
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            using var client = GetClient();
            return client.GetObjectStreamAsync(this.bucketName, path, null);
        }

        protected virtual async Task<IEnumerable<S3Object>> GetS3FilesAsync(string prefix = null, string delimiter = null)
        {
            using var client = GetClient();

            var items = new List<S3Object>();
            string nextKey = null;

            do
            {
                var response = await client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = this.bucketName,
                    Prefix = prefix,
                    Delimiter = delimiter,
                    MaxKeys = 1000,
                    ContinuationToken = nextKey
                });

                items.AddRange(response.S3Objects);
                nextKey = response.NextContinuationToken;

            } while (nextKey != null);

            return items;
        }

        public async Task<List<IDirectoryOrFile>> GetListAsync(string path)
        {
            var files = await this.GetFilesAsync(path);
            return files.Cast<IDirectoryOrFile>().ToList();
        }

        public async Task<IFileInfo> WriteFileAsync(string sourcePath, string path, bool overrideIfExists = true)
        {
            using var client = GetClient();
            await client.UploadObjectFromFilePathAsync(this.bucketName, path, sourcePath, null);
            var file = await GetFileInfoByPath(path);
            return file;
        }

        public Task<IFileInfo> WriteFileAsync(byte[] bytes, string path, bool overrideIfExists = true)
        {
            return WriteFileAsync(new MemoryStream(bytes), path, overrideIfExists: overrideIfExists);
        }

        public async Task<IFileInfo> WriteFileAsync(Stream stream, string path, bool overrideIfExists = true)
        {
            if (!overrideIfExists && await FileExistsAsync(path))
                throw new FileAlreadyExistsException(path);

            using var client = GetClient();
            var request = new PutObjectRequest
            {
                BucketName = this.bucketName,
                InputStream = stream,
                Key = path
            };

            var result = await client.PutObjectAsync(request);
            var file = await GetFileInfoByPath(path);
            return file;
        }

        private async Task<S3Object> GetS3FileByPath(string path)
        {
            var files = await this.GetS3FilesAsync(path);
            var s3o = files.FirstOrDefault();
            return s3o;
        }

        private async Task<IFileInfo> GetFileInfoByPath(string path)
        {
            var s3o = await GetS3FileByPath(path);
            if (s3o == null)
                throw new FileDoesNotExistException(path);

            var ret = new S3FileInfo(s3o);
            return ret;
        }
    }
}
