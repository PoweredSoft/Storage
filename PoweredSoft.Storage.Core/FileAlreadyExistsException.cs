using System;
using System.Runtime.Serialization;

namespace PoweredSoft.Storage.Core
{
    public class FileAlreadyExistsException : Exception
    {
        public FileAlreadyExistsException(string path) : base($"{path} already exists..")
        {
        }
    }
}