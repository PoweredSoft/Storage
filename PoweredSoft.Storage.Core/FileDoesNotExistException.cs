using System;

namespace PoweredSoft.Storage.Core
{
    public class FileDoesNotExistException : Exception
    {
        public FileDoesNotExistException(string path) : base($"{path} does not exist.")
        {

        }
    }
}